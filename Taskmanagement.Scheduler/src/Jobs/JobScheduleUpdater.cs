using Quartz;
using System;
using System.Threading.Tasks;
using Taskmanagement.Scheduler;
using Taskmanagement.Scheduler.Common;
using TaskManagement.DAL;

namespace EasyReportDispatcher_SCHEDULER.src.Jobs
{
    [DisallowConcurrentExecution]
    class JobScheduleUpdater : IJob
    {
        private string hashAutoSchedules = string.Empty;
        private string hashManSchedules = string.Empty;

        public object AppContextERD { get; private set; }

        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() => {

                {

                    //Preimposta reload con indicazione di forzatura
                    var bEseguiReload = (context.JobDetail.JobDataMap.Contains(CostantiSched.JobDataMap.System.ForceReloadSchedules) && Convert.ToBoolean(context.JobDetail.JobDataMap[CostantiSched.JobDataMap.System.ForceReloadSchedules]));

                    //Ricalcola hash schedulazioni
                    bEseguiReload |= this.checkSchedulesChanged();

                    //Se necessario reload procede
                    if (bEseguiReload)
                    {
                        this.updateSchedules();

                        AppContextTM.Service.InternalScheduler.Schedule_Last_Auto_Hash = this.hashAutoSchedules;
                        AppContextTM.Service.InternalScheduler.Schedule_Last_Manual_Hash = this.hashManSchedules;
                        AppContextTM.Service.InternalScheduler.Schedule_Last_Refresh = DateTime.Now;
                    }

                }

            });
        }

        private bool checkSchedulesChanged()
        {
            using (var slot = AppContextTM.Service.CreateSlot())
            {
                this.hashAutoSchedules = Query.CalculateAutoSchedulesHash(slot);
                this.hashManSchedules = Query.CalculateManualSchedulesHash(slot);
            }

            var changed = string.IsNullOrWhiteSpace(AppContextTM.Service.InternalScheduler.Schedule_Last_Auto_Hash);
            changed |= (this.hashAutoSchedules != AppContextTM.Service.InternalScheduler.Schedule_Last_Auto_Hash);
            changed |= (this.hashManSchedules != AppContextTM.Service.InternalScheduler.Schedule_Last_Manual_Hash);

            return (changed);
        }

        private void updateSchedules()
        {
            AppContextTM.Service.InternalScheduler.ReloadSchedules();
        }


    }
}
