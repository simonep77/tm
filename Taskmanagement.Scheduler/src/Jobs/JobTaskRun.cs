using Bdo.Objects;
using Quartz;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Taskmanagement.Scheduler;
using Taskmanagement.Scheduler.Common;
using TaskManagement.Common;
using TaskManagement.DAL;

namespace EasyReportDispatcher_SCHEDULER.src.Jobs
{
    class JobTaskRun : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() => {

                {
                    var ret = new JobResult();
                    context.Result = ret;

                    var planId = Convert.ToInt64(context.JobDetail.JobDataMap[CostantiSched.JobDataMap.Task.SchedPianoId]);
                    var taskDefId = Convert.ToInt32(context.JobDetail.JobDataMap[CostantiSched.JobDataMap.Task.TaskDeftId]);
                    var taskName = context.JobDetail.JobDataMap[CostantiSched.JobDataMap.Task.TaskName].ToString();

                    //Avvia esecuzione ed attende fine
                    var p = Process.Start(AppContextTM.TASKWORKER_EXE, planId.ToString());
                    p.WaitForExit();

                    //Riporta primo output
                    //marca piano come in esecuzione
                    using (var slot = AppContextTM.Service.CreateSlot())
                    {
                        var esito = slot.CreateList<TaskEsecuzioneLista>()
                        .OrderBy(nameof(TaskEsecuzione.Id))
                        .SearchByColumn(Filter.Eq(nameof(TaskEsecuzione.SchedPianoId), planId))
                        .GetFirst();

                        if (esito != null)
                        {
                            ret.IsOK = (esito.ReturnCode == 0);
                            ret.ReturnCode = esito.ReturnCode;
                            ret.ReturnMessage = esito.ReturnMessage;
                        }

                    }

                }

            });
        }


    }
}
