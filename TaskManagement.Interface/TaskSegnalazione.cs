using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManagement.Common;

namespace TaskInterfaceLib
{
    /// <summary>
    /// Rappresenta una condizione generata dal codice del task
    /// </summary>
    public class TaskSegnalazione: MarshalByRefObject
    {
        public ETipoSegnalazione Tipo { get; set; }
        public string ChiaveEntita{ get; set; }
        public string TipoEntita { get; set; }
        public string Testo { get; set; }

    }

}
