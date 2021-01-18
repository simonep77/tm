using Bdo.Objects;
using EasyReportDispatcher_SCHEDULER.src.Jobs;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public int Node_ID { get; set; }

        async public void Start()
        {
            var role = AppContextTM.SCHEDULE_MASTER_NODE ? "Ruolo: MASTER [Gestione piano schedulazione ed esecuzione dei task senza nodo assegnato]" : "Ruolo: SLAVE [esecuzione dei soli task assegnati esplicitamente]";
            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, @"Schedulatore interno job inizializzazione...");
            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, role);
            //Crea lo scheduler principale
            this.mScheduler = await (new StdSchedulerFactory()).GetScheduler();
            //Aggiunge listener per i task
            this.mScheduler.ListenerManager.AddJobListener(new JobListenerSystem(), GroupMatcher<JobKey>.GroupContains(JOB_INTERNAL_GROUP));
            this.mScheduler.ListenerManager.AddJobListener(new JobListenerTask(), GroupMatcher<JobKey>.GroupContains(JOB_TASKS_GROUP));
            await this.mScheduler.Start();

            //Registra proprio accesso
            this.caricaDatiNodo();

            //Crea e schedula i job di sistema
            this.avviaSchedulazioniSistema();

            // 3) Il job di check esiti schedulazione
            //trg = TriggerBuilder
            //    .Create()
            //    .WithIdentity(CostantiSched.Quartz.TriggerNames.System.ScheduleResultCheck, TRIGGER_INTERNAL_GROUP)
            //    .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(AppContextTM.SCHEDULE_RESULT_CHECK_MINUTES))
            //    //.StartAt(DateTime.Now.ToUniversalTime().AddMinutes(AppContextERD.SCHEDULE_RESULT_CHECK_MINUTES))
            //    .Build();

            //jobDett = new JobDetailImpl(CostantiSched.Quartz.JobNames.System.ScheduleResultCheck, JOB_INTERNAL_GROUP, typeof(JobScheduleResultChecker));
            //await this.mScheduler.ScheduleJob(jobDett, trg);


            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, @"Schedulatore interno job avviato");
        }

        #region GESTIONE NODO


        private void caricaDatiNodo()
        {
            using (var slot = AppContextTM.Service.CreateSlot())
            {

                var nodo = slot.LoadObjOrNewByKEY<TaskNodo>(TaskNodo.KEY_HOST, Environment.MachineName);

                if (nodo.ObjectState == EObjectState.New)
                {
                    nodo.Hostname = Environment.MachineName;

                    try
                    {
                        nodo.FQDN = System.Net.Dns.GetHostEntry("localhost").HostName;
                    }
                    catch (Exception ex)
                    {
                        AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Warning, $"Errore nell'ottenimento dell'FQDN: {ex.Message}");
                        nodo.FQDN = Environment.MachineName;
                    }

                    slot.SaveObject(nodo);

                    AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"Nodo registrato con successo. Id: {nodo.Id}");
                }
                else
                {
                    AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"Avvio nodo gia' registrato con Id: {nodo.Id}");
                }

                //Legge
                this.Node_ID = nodo.Id;
                //Aggiorna
                nodo.RunIsMaster = AppContextTM.SCHEDULE_MASTER_NODE ? (sbyte)1 : (sbyte)0;
                nodo.RunStart = DateTime.Now;
                nodo.RunEnd = DateTime.MinValue;
                nodo.RunPID = Process.GetCurrentProcess().Id.ToString();
                slot.SaveObject(nodo);
            }
        }

        private void terminaNodo()
        {
            using (var slot = AppContextTM.Service.CreateSlot())
            {
                var nodo = slot.LoadObjNullByKEY<TaskNodo>(TaskNodo.KEY_HOST, Environment.MachineName);

                if (nodo == null)
                    return;

                //Aggiorna
                nodo.RunEnd = DateTime.Now;
                slot.SaveObject(nodo);
            }
        }


        #endregion


        /// <summary>
        /// Il master node si occupa del piano di schedulazione
        /// </summary>
        async private void avviaSchedulazioniSistema()
        {
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
            await this.mScheduler.ScheduleJob(jobDett, trg);

        }



        async public void Stop()
        {
            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, @"Schedulatore interno job in chiusura...");
            //Chiude info nodo
            this.terminaNodo();
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


            if (AppContextTM.Service.RunMode == CostantiSched.RunMode.Console)
            {
                AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"Schedulazioni caricate: {trsOrdered.Count()}");
                AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, sb.ToString());
            }
            else
                AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"Schedulazioni caricate: {trsOrdered.Count()}");

        }



        private class TriggerStatus
        {
            public ITrigger Trigger;
            public TaskSchedulazionePiano Schedulazione;
        }

        async public void ReloadSchedules()
        {
            //Mette in pausa tutte le schedulazioni
            await this.mScheduler.Standby();
            try
            {


                var matcher = GroupMatcher<JobKey>.GroupContains(JOB_TASKS_GROUP);
                var jobkeys = await this.mScheduler.GetJobKeys(matcher);
                var trigDiz = new Dictionary<string, TriggerStatus>();

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

                    var dtPlanStart = DateTime.Now;
                    var dtPlanEnd = dtPlanStart.AddDays(AppContextTM.SCHEDULE_EXECUTION_PLAN_DAYS);

                    //Verifica essistenza ed aggiunge schedulazioni
                    foreach (var task in tasks)
                    {
                        try
                        {
                            var tBiz = task.ToBizObject<TaskDefinizioneBiz>();
                            //Viene utilizzata la notazione senza secondi NCrontab (Cron.guru)
                            var schedPlan = tBiz.ReBuildSchedulePlan(dtPlanEnd, AppContextTM.SCHEDULE_MASTER_NODE, this.Node_ID);

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
                        catch (Exception e)
                        {
                            AppContextTM.Service.WriteLog(EventLogEntryType.Error, $"Errore nella schedulazione del task: {task.Nome}");
                            AppContextTM.Service.SendMailError($"Errore nella schedulazione del task: {task.Nome}<br>{e.Message}<br><br>{e.StackTrace}");
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

                this.printSchedules();

            }
            catch (Exception e)
            {
                AppContextTM.Service.WriteLog(EventLogEntryType.Error, $"Errore nell'aggiornamento del piano di schedulazione");
                AppContextTM.Service.SendMailError($"Errore nell'aggiornamento del piano di schedulazione: <br>{e.Message}<br><br>{e.StackTrace}");
            }
            finally
            {
                //Riavvia tutte le schedulazioni
                await this.mScheduler.Start();
            }

        }
    }
}
