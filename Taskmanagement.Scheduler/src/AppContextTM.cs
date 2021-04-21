using Bdo.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskmanagement.Scheduler.Svcs;
using Taskmanagement.Scheduler.Properties;
using System.Reflection;

namespace Taskmanagement.Scheduler
{
    public class AppContextTM
    {
        public const string SERVICE_NAME = @"TM-Scheduler";
        public const string LOG_EVENT_SOURCE = @"TM_Scheduler";
        public const string LOG_EVENT_SOURCE_LOG = @"TM Scheduler Eventlog";

        public static readonly string SCHEDULE_EXTEND_PLAN_CRONSTRING = @"0 7 0 ? * * *";
        public static readonly int SCHEDULE_FORCED_UPDATE_CHECK_SECONDS = Settings.Default.SecondiCheckModificheSchedulazioni;
        public static readonly int SCHEDULE_RESULT_CHECK_MINUTES = 10;
        public static readonly int SCHEDULE_EXECUTION_PLAN_DAYS = 2;
        public static readonly bool SCHEDULE_MASTER_NODE = Settings.Default.MasterNode;
        public static readonly string API_BINDING_ADDRESS = Settings.Default.ApiBindingAddress;

        public static readonly string TASKWORKER_EXE = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"TaskManagement.Worker.exe");

        public static string UserDataDir { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TM");
        public static string UserDataDirOutput { get; } = Path.Combine(UserDataDir, "Output");
        public static string UserDataDirTemplate { get; } = Path.Combine(UserDataDir, "Template");

        public static MainService Service { get; } = new MainService();




    }
}
