using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskManagement.Interface
{
    public interface ITaskSegnalazione
    {

        int Tipo { get; set; }
        string ChiaveEntita { get; set; }
        string TipoEntita { get; set; }
        string Testo { get; set; }


    }
}
