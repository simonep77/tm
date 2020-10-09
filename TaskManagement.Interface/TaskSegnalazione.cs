using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManagement.Common;
using TaskManagement.Interface;

namespace TaskManagement.Interface
{
    /// <summary>
    /// Rappresenta una condizione generata dal codice del task
    /// </summary>
    public class TaskSegnalazione: MarshalByRefObject, ITaskSegnalazione
    {
        public int Tipo { get; set; }
        public string ChiaveEntita{ get; set; }
        public string TipoEntita { get; set; }
        public string Testo { get; set; }

    }

}
