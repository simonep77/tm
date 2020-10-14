using Bdo.Objects;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.DAL;

namespace EasyReportDispatcher_SCHEDULER.src.Jobs
{
    public class JobResult
    {
        public bool IsOK;
        public string Message = string.Empty;
        public int ReturnCode;
        public string ReturnMessage = string.Empty;
    }
}
