using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskManagement.Interface
{
    public class TaskRuntimeParametro :MarshalByRefObject, ITaskRuntimeParametro
    {
        public string Chiave { get; }
        public string Valore { get; }
        public bool IsVisibile { get; }

        public TaskRuntimeParametro(string chiave, string valore, bool visibile)
        {
            this.Chiave = chiave;
            this.Valore = valore;
            this.IsVisibile = visibile;
        }
    }
}
