using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManagement.Common;

namespace TaskInterfaceLib
{
    /// <summary>
    /// Informazioni di esecuzune task
    /// </summary>
    public class TaskRuntimeinfo: MarshalByRefObject
    {
        public int SysTaskId { get; set; }
        public string SysTaskName { get; set; }
        public string SysAssemblyPath { get; set; }
        public string SysTaskClass { get; set; }
        public string SysLogDir { get; set; }
        public string SysDatiDir { get; set; }
        public string SysRiferimento { get; set; }
        public string SysNote { get; set; }
        public string SysSistema { get; set; }
        public int SysTipoNotifica { get; set; }
        public string SysMailFROM { get; set; }
        public string SysMailTO { get; set; }
        public string SysMailCC { get; set; }
        public string SysMailBCC { get; set; }
        public ILogger Logger { get; set; }
        public string LogFileName { get; set; }
        public int TaskPID { get; set; }
        public DateTime TaskStartDate { get; set; }
        public DateTime TaskEndDate { get; set; }
        public int TaskLastReturnCode { get; set; }
        public string TaskLastMessage { get; set; }
        public ListaParametri UserParams { get; } = new ListaParametri();

        /// <summary>
        /// Segnalazioni applicative fornite durante l'esecuzione
        /// </summary>
        public List<TaskSegnalazione> Segnalazioni = new List<TaskSegnalazione>();

        /// <summary>
        /// Indica se presente almeno una segnalazione di errore
        /// </summary>
        public bool HaSegnalazioniErrore
        {
            get
            {
                return this.Segnalazioni.Where(s => s.Tipo == ETipoSegnalazione.Error).Any();
            }
        }

        /// <summary>
        /// Indica se presente almeno una segnalazione di warning
        /// </summary>
        public bool HaSegnalazioniWarning
        {
            get
            {
                return this.Segnalazioni.Where(s => s.Tipo == ETipoSegnalazione.Warning).Any();
            }
        }

        public void InitLogger()
        {
            if (this.Logger != null)
                throw new ApplicationException(@"Impossibile inizializzare un logger gia' istanziato");

            this.Logger = new FileLogger(this.LogFileName);
        }
    }

}
