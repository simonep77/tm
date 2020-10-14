using Bdo.Objects;

using LevelB.Vici.WinService.Service;
using LevelB.Vici.WinService.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Taskmanagement.Scheduler.Common;

namespace Taskmanagement.Scheduler.Svcs
{
    public class MainService : Service
    {
        private Mutex mMutex;

        public int RunMode { get; set; }

        public IntSvcScheduler InternalScheduler { get; } = new IntSvcScheduler();

        public MainService(): base(@"TM-Scheduler")
        {
            this.ServiceInfo.DisplayName = @"Task management Scheduler";
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


        #region PRIVATE

        /// <summary>
        /// Crea mutex per esecuzione a singola istanza
        /// </summary>
        private void checkOneInstance()
        {
            bool createdNew;

            this.mMutex = new Mutex(true, AppContextTM.SERVICE_NAME, out createdNew);

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
