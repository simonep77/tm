using Bdo.Objects;
using LevelB.Arch.Core.Extensions;
using Quartz;
using System.Linq;
using System.Threading.Tasks;
using Taskmanagement.Scheduler;
using Taskmanagement.Scheduler.Common;
using Taskmanagement.Scheduler.Svcs;
using TaskManagement.Common;
using TaskManagement.DAL;

namespace EasyReportDispatcher_SCHEDULER.src.Jobs
{
    [DisallowConcurrentExecution]
    class JobScheduleResultChecker : IJob
    {
        private string hashSchedules = string.Empty;

        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(async () =>
            {

                {
                    //
                    using (var slot = AppContextTM.Service.CreateSlot())
                    {
                        var runJobs = await context.Scheduler.GetCurrentlyExecutingJobs();

                        var runScheds = slot.CreateList<TaskSchedulazionePianoLista>()
                            .SearchByColumn(Filter.Eq(nameof(TaskSchedulazionePiano.StatoEsecuzioneId), EStatoEsecuzione.PS_InEsecuzione));


                        var zombies = runScheds.Where(s =>
                        {
                            return !runJobs.Where(j => j.JobDetail.Key.Group == IntSvcScheduler.JOB_TASKS_GROUP).Where(j => (int)j.JobDetail.JobDataMap[CostantiSched.JobDataMap.Task.TaskDeftId] == s.TaskDefId).Any();
                        });

                        if (zombies.Any())
                        {
                            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"Trovate {zombies.Count()} schedulazioni senza esito.");

                            //OK, elimina
                            foreach (var item in zombies)
                            {
                                item.StatoEsecuzioneId = EStatoEsecuzione.PS_TerminatoConErrori;
                                slot.SaveObject(item);
                            }
                        }
                    }
                }

            });

        }
    }
}
