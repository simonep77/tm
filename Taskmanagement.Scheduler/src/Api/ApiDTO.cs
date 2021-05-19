using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskmanagement.Scheduler.Api
{
    public class TaskScheduleDTO
    {
        public long RunId { get; set; }
        public int StatoId { get; set; }
        public string StatoDesc { get; set; }
        public string DataEsecuzione { get; set; }
        public string TaskNome { get; set; }
    }


    public class TaskScheduleRunInfoDTO: TaskScheduleDTO
    {
        public string DataAvvio { get; set; }
        public string DataTermine { get; set; }
        public int ReturnCode { get; set; }
        public string ReturnMessage { get; set; }

    }


}
