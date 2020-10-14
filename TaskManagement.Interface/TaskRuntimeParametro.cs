using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskManagement.Interface
{
    public class TaskRuntimeParametro :MarshalByRefObject, ITaskRuntimeParametro
    {
        public string Chiave { get; set; }
        public string Valore { get; set; }
        public string ValoreOpzionale { get; set; } = string.Empty;
        public bool IsVisibile { get; set; }
        public bool IsCondiviso { get; set; }
        public bool IsOverride { get; set; }


    }
}
