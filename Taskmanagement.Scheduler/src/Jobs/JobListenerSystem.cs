using Bdo.Utils;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;
using Taskmanagement.Scheduler;
using Taskmanagement.Scheduler.Properties;

namespace EasyReportDispatcher_SCHEDULER.src.Jobs
{
    class JobListenerSystem : IJobListener
    {
        public string Name => @"JobSystemListener1";


        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            //throw new NotImplementedException();
            return Task.Run(()=> { });
        }

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => { });
            //return Task.Run(() => {
            //    AppContextTM.Service.WriteLog( System.Diagnostics.EventLogEntryType.Information,$" > Avvio job [sistema] {context.JobDetail.Key.Name}");
            //});

        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => {

                 if (jobException != null)
                {
                    Exception e = jobException;

                    while (e.InnerException != null)
                        e = e.InnerException;

                    //Logga
                    AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Error, 
                        $"Si è verificato il seguente errore nel job {context.JobInstance.GetType().Name}: {e.Message}");

                    //Invia email
                    if (!string.IsNullOrWhiteSpace(Settings.Default.NotificaErroriApplicazioneTO))
                    {
                        var subj = $"ERR - {AppContextTM.SERVICE_NAME} - {context.JobInstance.GetType().Name}";
                        var body = $"Si è verificato il seguente errore:<br/>{e.Message}<br/><br/>{e.StackTrace}";
                        var ml = new Mailer();
                        ml.Send(null, Settings.Default.NotificaErroriApplicazioneTO, Settings.Default.NotificaErroriApplicazioneCC, null, subj, body, null, null);
                    }

                }

            });
        
        
        }
    }
}
