using Bdo.Objects;
using EasyReportDispatcher_SCHEDULER.src.Jobs;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taskmanagement.Scheduler.Common;
using TaskManagement.BIZ.src;
using TaskManagement.DAL;

namespace Taskmanagement.Scheduler.Svcs
{
    public class IntSvcScheduler
    {
        public const string JOB_INTERNAL_GROUP = @"InternalJobs";
        public const string JOB_TASKS_GROUP = @"TaskSchedJobs";
        public const string TRIGGER_INTERNAL_GROUP = @"InternalTrig";
        public const string TRIGGER_TASKS_GROUP = @"TaskSchedTrig";

        private object mMainSync = new object();
        private IScheduler mScheduler;
        public string Schedule_Last_Auto_Hash { get; set; } = string.Empty;
        public string Schedule_Last_Manual_Hash { get; set; } = string.Empty;
        public DateTime Schedule_Last_Refresh { get; set; } = DateTime.MinValue;


        async public void Start()
        {
            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, @"Schedulatore interno job inizializzazione...");
            //Crea lo scheduler principale
            this.mScheduler = await (new StdSchedulerFactory()).GetScheduler();
            //Aggiunge listener per i task
            this.mScheduler.ListenerManager.AddJobListener(new JobListenerSystem(), GroupMatcher<JobKey>.GroupContains(JOB_INTERNAL_GROUP));
            this.mScheduler.ListenerManager.AddJobListener(new JobListenerTask(), GroupMatcher<JobKey>.GroupContains(JOB_TASKS_GROUP));
            await this.mScheduler.Start();
            
            //Crea e schedula i job di sistema

            // 1) Il job di caricamento schedulazioni per garantire l'estensione del piano di esecuzione
            var trg = TriggerBuilder
                .Create()
                .WithIdentity(CostantiSched.Quartz.TriggerNames.System.ScheduleExtender, TRIGGER_INTERNAL_GROUP)
                .WithCronSchedule(AppContextTM.SCHEDULE_EXTEND_PLAN_CRONSTRING)
                .Build();

            var jobDett = new JobDetailImpl(CostantiSched.Quartz.JobNames.System.ScheduleExtender, JOB_INTERNAL_GROUP, typeof(JobScheduleUpdater));
            jobDett.JobDataMap.Add(CostantiSched.JobDataMap.System.ForceReloadSchedules, true);

            await this.mScheduler.ScheduleJob(jobDett, trg);

            // 2) Il job di check presenza modifiche alle schedulazioni (che parte comunque immediatamente)
            trg = TriggerBuilder
                .Create()
                .WithIdentity(CostantiSched.Quartz.TriggerNames.System.ScheduleUpdateCheck, TRIGGER_INTERNAL_GROUP)
                .WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForever(AppContextTM.SCHEDULE_FORCED_UPDATE_CHECK_SECONDS))
                .Build();

            jobDett = new JobDetailImpl(CostantiSched.Quartz.JobNames.System.ScheduleUpdateCheck, JOB_INTERNAL_GROUP, typeof(JobScheduleUpdater));
            await this.mScheduler.ScheduleJob( jobDett , trg);


            // 3) Il job di check esiti schedulazione
            trg = TriggerBuilder
                .Create()
                .WithIdentity(CostantiSched.Quartz.TriggerNames.System.ScheduleResultCheck, TRIGGER_INTERNAL_GROUP)
                .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(AppContextTM.SCHEDULE_RESULT_CHECK_MINUTES))
                //.StartAt(DateTime.Now.ToUniversalTime().AddMinutes(AppContextERD.SCHEDULE_RESULT_CHECK_MINUTES))
                .Build();

            jobDett = new JobDetailImpl(CostantiSched.Quartz.JobNames.System.ScheduleResultCheck, JOB_INTERNAL_GROUP, typeof(JobScheduleResultChecker));
            await this.mScheduler.ScheduleJob(jobDett, trg);


            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, @"Schedulatore interno job avviato");
        }

        async public void Stop()
        {
            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, @"Schedulatore interno job in chiusura...");
            //Stoppa schedulatore
            await this.mScheduler.PauseAll();
            //await this.mScheduler.Clear();
            await this.mScheduler.Shutdown(true);

            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, @"Schedulatore interno job terminato");
        }


        async public void DeleteAllReportSchedules()
        {
            var matcher = GroupMatcher<JobKey>.GroupContains(JOB_TASKS_GROUP);
            var jobkeys = await this.mScheduler.GetJobKeys(matcher);

            await this.mScheduler.DeleteJobs(jobkeys);
        }

        private string CalcTrigName(int taskDefId, DateTime date)
        {
            return $"T_{taskDefId}_dt_{date:yyyyMMddHHmm}";
        }

        private string CalcJobName(int taskDefId, DateTime date)
        {
            return $"J_{taskDefId}_dt_{date:yyyyMMddHHmm}";
        }


        async private void printSchedules()
        {
            var matcher = GroupMatcher<JobKey>.GroupContains(JOB_TASKS_GROUP);
            var jobkeys = await this.mScheduler.GetJobKeys(matcher);

            var trigList = new List<ITrigger>();

            var sb = new StringBuilder();
            sb.AppendLine(@"Elenco prossime schedulazioni: ");

            foreach (var key in jobkeys)
            {
                trigList.AddRange(await this.mScheduler.GetTriggersOfJob(key));
            }

            var trsOrdered = trigList.OrderBy(t => t.StartTimeUtc);

            foreach (var tr in trsOrdered)
            {
                var job = await this.mScheduler.GetJobDetail(tr.JobKey);
                sb.AppendFormat($" > {tr.StartTimeUtc.ToLocalTime():dd/MM/yyyy HH:mm} -  {job.JobDataMap[CostantiSched.JobDataMap.Task.TaskName]}");
                sb.AppendLine();
            }

            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"Schedulazioni caricate: {trsOrdered.Count()}");

        }



        private class TriggerStatus
        {
            public ITrigger Trigger;
            public TaskSchedulazionePiano Schedulazione;
        }

        async public void ReloadReportSchedules()
        {
            //Mette in pausa tutte le schedulazioni
            await this.mScheduler.Standby();
            try
            {


                var matcher = GroupMatcher<JobKey>.GroupContains(JOB_TASKS_GROUP);
                var jobkeys = await this.mScheduler.GetJobKeys(matcher);
                var trigDiz = new Dictionary<string, TriggerStatus>();
                int iNumSched = 0;

                using (var slot = AppContextTM.Service.CreateSlot())
                {

                    //Carichiamo i trigger == job IN MEMORIA

                    foreach (var key in jobkeys)
                    {
                        foreach (var trg in await this.mScheduler.GetTriggersOfJob(key))
                        {
                            trigDiz.Add(trg.Key.Name, new TriggerStatus() { Trigger=trg });
                        }
                    }
                    


                    var tasks = slot.CreateList<TaskDefinizioneLista>()
                        .LoadFullObjects()
                        .SearchByColumn(Filter.Eq(nameof(TaskDefinizione.Attivo), 1)
                        .And(Filter.NotNull(nameof(TaskDefinizione.SchedCronString)).Or(Filter.Neq(nameof(TaskDefinizione.SchedCronString), string.Empty)))
                        .And(Filter.Lte(nameof(TaskDefinizione.DataInizio), DateTime.Today)
                        .And(Filter.Gte(nameof(TaskDefinizione.DataFine), DateTime.Today))));

                    iNumSched = tasks.Count;
                    var dtPlanStart = DateTime.Now;
                    var dtPlanEnd = dtPlanStart.AddDays(AppContextTM.SCHEDULE_EXECUTION_PLAN_DAYS);

                    //Verifica essistenza ed aggiunge schedulazioni
                    foreach (var task in tasks)
                    {
                        var tBiz = task.ToBizObject<TaskDefinizioneBiz>();
                        //Viene utilizzata la notazione senza secondi NCrontab (Cron.guru)
                        var schedPlan = tBiz.ReBuildSchedulePlan(dtPlanEnd);

                        foreach (var piano in schedPlan)
                        {
                            //Crea nome trigger
                            var trName = this.CalcTrigName(task.Id, piano.DataEsecuzione);

                            TriggerStatus trStatus = null;

                            //Verifica esistenza: se non esiste lo crea, se esiste lo rimuove da elenco
                            if (!trigDiz.TryGetValue(trName, out trStatus))
                            {
                                //Crea trigger
                                var trg = TriggerBuilder.Create().StartAt(piano.DataEsecuzione.ToUniversalTime()).WithIdentity(trName, TRIGGER_TASKS_GROUP).Build();
                                
                                //Crea job
                                var jobDet = new JobDetailImpl(this.CalcJobName(piano.TaskDefId, piano.DataEsecuzione), JOB_TASKS_GROUP, typeof(JobTaskRun));
                                jobDet.JobDataMap.Add(CostantiSched.JobDataMap.Task.SchedPianoId, piano.Id);
                                jobDet.JobDataMap.Add(CostantiSched.JobDataMap.Task.TaskDeftId, task.Id);
                                jobDet.JobDataMap.Add(CostantiSched.JobDataMap.Task.TaskName, task.Nome);
                                //Schedula
                                await this.mScheduler.ScheduleJob(jobDet, trg);
                            }
                            else
                            {
                                //Rimuove da elenco in quanto esistente dei trigger in memoria
                                trigDiz.Remove(trName);
                            }
                        }
                    }

                    //Rimuove i job non piu' esistenti/coerenti
                    foreach (var item in trigDiz)
                    {
                        //if (item.Value.Schedulazione.ObjectState == EObjectState.Loaded)
                        //    slot.DeleteObject(item.Value.Schedulazione);

                        await this.mScheduler.DeleteJob(item.Value.Trigger.JobKey);
                    }


                    trigDiz.Clear();

                }

                if (AppContextTM.Service.RunMode == CostantiSched.RunMode.Console)
                    this.printSchedules();
                else
                    AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"Caricate {iNumSched} schedulazioni");
            }
            finally
            {
                //Riavvia tutte le schedulazioni
                await this.mScheduler.Start();
            }

        }
    }
}
