using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskmanagement.Scheduler.Common
{
    public static class CostantiSched
    {
        public static class RunMode
        {
            public const int Service = 0;
            public const int Console = 1;
            public const int Install = 2;
            public const int Uninstall = 3;
        }




        public static class Quartz
        {

            public static class JobNames
            {
                public static class System
                {
                    public const string ScheduleExtender = @"JobScheduleExtender";
                    public const string ScheduleUpdateCheck = @"JobScheduleUpdateCheck";
                    public const string ScheduleResultCheck = @"JobScheduleResultCheck";

                }

                public static class Reports
                {
                    //public const string ReportId = @"ReportId";
                    //public const string ReportName = @"ReportName";

                }
            }

            public static class TriggerNames
            {
                public static class System
                {
                    public const string ScheduleExtender = @"TrigScheduleExtend";
                    public const string ScheduleUpdateCheck = @"TrigScheduleUpdateCheck";
                    public const string ScheduleResultCheck = @"TrigScheduleResultCheck";

                }
            }

        }


        public static class JobDataMap
        {
            public static class System
            {
                public const string ForceReloadSchedules = @"ForceReloadSchedules";

            }

            public static class Task
            {
                public const string SchedPianoId = @"PianoId";
                public const string TaskDeftId = @"TaskDefId";
                public const string TaskName = @"TaskName";

                /// <summary>
                /// Chiave da utilizzare per la gestione delle estrazioni concorrenti (stessa connection)
                /// </summary>
                public const string ConcurrencyKey = @"ConcurrencyKey";

            }
        }

    }
}
