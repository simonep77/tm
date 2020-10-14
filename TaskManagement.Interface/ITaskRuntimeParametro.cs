using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskManagement.Interface
{
    public interface ITaskRuntimeParametro
    {
        string Chiave { get; set; }
        string Valore { get; set; }
        string ValoreOpzionale { get; set; }
        bool IsCondiviso { get; set; }
        bool IsVisibile { get; set; }
        bool IsOverride { get; set; }

    }
}
