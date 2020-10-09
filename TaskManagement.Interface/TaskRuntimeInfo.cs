using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManagement.Common;
using TaskManagement.Interface;

namespace TaskManagement.Interface
{
    /// <summary>
    /// Informazioni di esecuzune task
    /// </summary>
    public class TaskRuntimeInfo: MarshalByRefObject, ITaskRunTimeInfo
    {
        public int SysTaskId { get; set; }
        public string SysTaskName { get; set; }
        public string SysTaskType { get; set; }
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
        public string LogFileName { get; set; }
        public int TaskPID { get; set; }
        public DateTime TaskStartDate { get; set; }
        public DateTime TaskEndDate { get; set; }
        public int TaskLastReturnCode { get; set; }
        public string TaskLastMessage { get; set; }
        public ITaskRuntimeParametroLista UserParams { get; } = new TaskRuntimeParametroLista();

        /// <summary>
        /// Segnalazioni applicative fornite durante l'esecuzione
        /// </summary>
        public List<ITaskSegnalazione> Segnalazioni { get; } = new List<ITaskSegnalazione>();

        /// <summary>
        /// Indica se presente almeno una segnalazione di errore
        /// </summary>
        public bool HaSegnalazioniErrore
        {
            get
            {
                return this.Segnalazioni.Where(s => s.Tipo == (int)ETipoSegnalazione.Error).Any();
            }
        }

        /// <summary>
        /// Indica se presente almeno una segnalazione di warning
        /// </summary>
        public bool HaSegnalazioniWarning
        {
            get
            {
                return this.Segnalazioni.Where(s => s.Tipo == (int)ETipoSegnalazione.Warning).Any();
            }
        }


    }

}
