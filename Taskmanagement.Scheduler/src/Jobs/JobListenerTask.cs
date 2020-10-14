using Quartz;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Taskmanagement.Scheduler;
using Taskmanagement.Scheduler.Common;
using TaskManagement.Common;
using TaskManagement.DAL;

namespace EasyReportDispatcher_SCHEDULER.src.Jobs
{
    class JobListenerTask : IJobListener
    {
        public string Name => @"JobReportListener1";

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            //throw new NotImplementedException();
            return Task.Run(()=> { });
        }

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => {
                AppContextTM.Service.WriteLog( System.Diagnostics.EventLogEntryType.Information,$" > Avvio job [task] n.{context.JobDetail.JobDataMap[CostantiSched.JobDataMap.Task.TaskDeftId].ToString().PadLeft(4,'0')} - {context.JobDetail.JobDataMap[CostantiSched.JobDataMap.Task.TaskName]}");
            });

        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => {

                var planId = Convert.ToInt64(context.JobDetail.JobDataMap[CostantiSched.JobDataMap.Task.SchedPianoId]);
                var taskDefId = Convert.ToInt32(context.JobDetail.JobDataMap[CostantiSched.JobDataMap.Task.TaskDeftId]);
                var taskName = context.JobDetail.JobDataMap[CostantiSched.JobDataMap.Task.TaskName].ToString();

                var ret = context.Result as JobResult;
                var logType = EventLogEntryType.Information;
                var errText = string.Empty;

                if (ret == null)
                {
                    logType = EventLogEntryType.Warning;
                }
                else if (jobException != null)
                {
                    logType = EventLogEntryType.Error;
                    Exception ex = jobException;
                    while (ex != null)
                    {
                        errText += ex.Message + " - ";
                        ex = ex.InnerException;
                    }
                    
                }
                else
                {
                    if (!ret.IsOK)
                    {
                        logType = EventLogEntryType.Error;
                        errText = ret.Message;
                    }
                }

                AppContextTM.Service.WriteLog(logType, $" > Termine job [report] n.{taskDefId} - {taskName}" + (errText.Length == 0 ? string.Empty : string.Concat("\n", errText)));

            });
        }
    }
}
