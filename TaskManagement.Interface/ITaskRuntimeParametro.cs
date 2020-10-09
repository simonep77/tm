using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskManagement.Interface
{
    public interface ITaskRuntimeParametro
    {
        string Chiave { get; }
        string Valore { get; }
        bool IsVisibile { get; }

    }
}
