using Bdo.Objects;
using Bdo.Utils;
using LevelB.Vici.WinService.Service;
using LevelB.Vici.WinService.Tasks;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Taskmanagement.Scheduler.Common;
using Taskmanagement.Scheduler.Properties;
using Taskmanagement.Scheduler.src.Api;
using TaskManagement.DAL;

namespace Taskmanagement.Scheduler.Svcs
{
    public class MainService : Service
    {
        private Mutex mMutex;

        public int RunMode { get; set; }

        public IntSvcScheduler InternalScheduler { get; } = new IntSvcScheduler();
        protected IDisposable InternalApiWebApp { get; set; }

        public MainService(): base(@"TM-Scheduler")
        {
            this.ServiceInfo.DisplayName = @"Task Management Scheduler";
            this.ServiceInfo.Description = @"Servizio residente per la gestione dei task schedulati TM";
            this.ServiceInfo.ServiceAccount = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.ServiceInfo.ServiceStartMode = System.ServiceProcess.ServiceStartMode.Automatic;
            this.ServiceInfo.DependsOn = new string[] { @"tcpip" };
        }

        protected override void OnStarted()
        {
            base.OnStarted();
        }

        protected override void OnStarting()
        {
            base.OnStarting();

            this.initEventLog();

            this.InternalScheduler.Start();

            this.startWebApi();

        }

        protected override void OnStateChanged(ServiceTask serviceTask, ServiceState serviceState)
        {
            base.OnStateChanged(serviceTask, serviceState);
        }

        protected override void OnStopped()
        {
            base.OnStopped();
        }

        protected override void OnStopping()
        {
            base.OnStopping();

            this.InternalScheduler.Stop();

            this.stopWebApi();

            if (this.mMutex != null)
                this.mMutex.Dispose();
        }

        protected override void OnTaskException(ServiceTask serviceTask, Exception e)
        {
            base.OnTaskException(serviceTask, e);

            this.WriteLog(System.Diagnostics.EventLogEntryType.Error, e.Message);
        }

        protected override void OnTaskStarted(ServiceTask serviceTask)
        {
            base.OnTaskStarted(serviceTask);
        }

        protected override void OnTaskStarting(ServiceTask serviceTask)
        {
            base.OnTaskStarting(serviceTask);
        }

        protected override void OnTaskStopped(ServiceTask serviceTask)
        {
            base.OnTaskStopped(serviceTask);
        }

        protected override void OnTaskStopping(ServiceTask serviceTask)
        {
            base.OnTaskStopping(serviceTask);
        }


        #region PUBLIC


        public void RunByMode(int mode)
        {
            this.RunMode = mode;

            try
            {
                this.checkOneInstance();

                switch (mode)
                {
                    case CostantiSched.RunMode.Service:
                        //Service
                        this.Run();
                        break;
                    case CostantiSched.RunMode.Console:
                        //Console
                        this.RunConsole();
                        break;
                    case CostantiSched.RunMode.Install:
                        this.Install();
                        break;
                    case CostantiSched.RunMode.Uninstall:
                        this.UnInstall();
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                this.WriteLog(EventLogEntryType.Error, ex.Message);
            }

            
        }

        /// <summary>
        /// Crea gli slot da utilizzare di volta in volta
        /// </summary>
        /// <returns></returns>
        public BusinessSlot CreateSlot()
        {
            var bs = new BusinessSlot("Default");
            bs.LiveTrackingEnabled = false;
            bs.ChangeTrackingEnabled = true;
            bs.DB.AutoCloseConnection = true;

            return bs;
        }

        #endregion


        public void WriteLog(EventLogEntryType logType, string logMessage)
        {

            if (this.RunMode == CostantiSched.RunMode.Service)
            {
                EventLog.WriteEntry(AppContextTM.LOG_EVENT_SOURCE, logMessage, logType);
            }
            else
            {
                var dtNow = DateTime.Now;

                Console.Write(string.Format($"{dtNow:yyyy-MM-dd HH:mm:ss} - {Enum.GetName(typeof(EventLogEntryType), logType)} - "));
                Console.WriteLine(logMessage);
            }

        }


        /// <summary>
        /// Invia una mail di errore
        /// </summary>
        /// <param name="body"></param>
        public void SendMailError(string body)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Settings.Default.NotificaErroriApplicazioneTO))
                {
                    var subj = $"ERR - {AppContextTM.SERVICE_NAME}";
                    var bodyH = $"Si è verificato il seguente errore:<br/>" + body;
                    var ml = new Mailer();
                    ml.Send(null, Settings.Default.NotificaErroriApplicazioneTO, Settings.Default.NotificaErroriApplicazioneCC, null, subj, body, null, null);
                }
            }
            catch (Exception ex)
            {
                this.WriteLog(EventLogEntryType.Error, $"Errore nell'invio email di notifica errore: {ex.Message}");
            }
        }

        /// <summary>
        /// Esegue task dal nome
        /// </summary>
        /// <param name="taskName"></param>
        /// <param name="waitEnd"></param>
        public void TaskRunByName(string taskName, bool waitEnd)
        {
            //Avvia esecuzione ed attende fine
            var p = Process.Start(AppContextTM.TASKWORKER_EXE, taskName);

            if (waitEnd)
                p.WaitForExit();
        }

        /// <summary>
        /// Esegue task da piano di schedulazione
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="waitEnd"></param>
        public void TaskRunByPlanId(long planId, bool waitEnd)
        {
            //Avvia esecuzione ed attende fine
            var p = Process.Start(AppContextTM.TASKWORKER_EXE, planId.ToString());

            if (waitEnd)
                p.WaitForExit();
        }


        #region PRIVATE

        private void startWebApi()
        {
            if (AppContextTM.API_ENABLED)
                this.WriteLog(EventLogEntryType.Information, "Avvio WebApi...");
            else
            {
                this.WriteLog(EventLogEntryType.Information, "WebApi non abilitate");
                return;
            }

            try
            {
                //string baseAddress = "http://localhost:9000/";
                this.InternalApiWebApp = WebApp.Start<StartupApi>(AppContextTM.API_BINDING_ADDRESS);
                this.WriteLog(EventLogEntryType.Information, $"Web Api avviate su: {AppContextTM.API_BINDING_ADDRESS}");
            }
            catch (Exception e)
            {
                while (e != null)
                {
                    this.WriteLog(EventLogEntryType.Error, $"Errore nell'attivazione delle Web Api: {e.Message}");
                    e = e.InnerException;
                }

                //Istruzioni
                this.WriteLog(EventLogEntryType.Warning, "Istruzioni in caso di errore da 'accesso negato':");
                this.WriteLog(EventLogEntryType.Warning, " - Eseguire come amministratore il seguente comando:");
                this.WriteLog(EventLogEntryType.Warning, $"   netsh http add urlacl url={AppContextTM.API_BINDING_ADDRESS.Replace('+','*')} user=Everyone listen = yes");

                //netsh http add urlacl url=http://example.com:8080/ user=Everyone listen = yes

                this.SendMailError($"Errore nell'attivazione delle Web Api: {e.Message}");
            }
        }

        private void stopWebApi()
        {
            if (AppContextTM.API_ENABLED)
                this.WriteLog(EventLogEntryType.Information, "Stop WebApi...");

            this.InternalApiWebApp?.Dispose();
        }


        /// <summary>
        /// Crea mutex per esecuzione a singola istanza
        /// </summary>
        private void checkOneInstance()
        {
            bool createdNew;

            this.mMutex = new Mutex(true, $"Global\\{AppContextTM.SERVICE_NAME}" , out createdNew);

            if (!createdNew)
                throw new ApplicationException("E' già in esecuzione un'altra istanza dell'applicazione");
        }


        private void initEventLog()
        {
            if (this.RunMode == CostantiSched.RunMode.Service)
            {
                if (!EventLog.SourceExists(AppContextTM.LOG_EVENT_SOURCE))
                {
                    EventLog.CreateEventSource(AppContextTM.LOG_EVENT_SOURCE, AppContextTM.LOG_EVENT_SOURCE_LOG);
                }
            }
        }



        #endregion
    }
}
