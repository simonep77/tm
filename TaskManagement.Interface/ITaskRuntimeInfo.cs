using System;
using System.Collections.Generic;
using TaskManagement.Common;
using TaskManagement.Interface;

namespace TaskManagement.Interface
{
    /// <summary>
    /// Interfaccia base per la definizione dei dati di runtime del task
    /// </summary>
    /// <remarks></remarks>
    /// 
    public interface ITaskRunTimeInfo
    {
         int SysTaskId { get; set; }
         string SysTaskName { get; set; }
         string SysAssemblyPath { get; set; }
         string SysTaskClass { get; set; }
         string SysLogDir { get; set; }
         string SysDatiDir { get; set; }
         string SysRiferimento { get; set; }
         string SysNote { get; set; }
         string SysSistema { get; set; }
         int SysTipoNotifica { get; set; }
         string SysMailFROM { get; set; }
         string SysMailTO { get; set; }
         string SysMailCC { get; set; }
         string SysMailBCC { get; set; }
         string LogFileName { get; set; }
         int TaskPID { get; set; }
         DateTime TaskStartDate { get; set; }
         DateTime TaskEndDate { get; set; }
         int TaskLastReturnCode { get; set; }
         string TaskLastMessage { get; set; }
         ITaskRuntimeParametroLista UserParams { get; }

        /// <summary>
        /// Segnalazioni applicative fornite durante l'esecuzione
        /// </summary>
        List<ITaskSegnalazione> Segnalazioni { get; }

        /// <summary>
        /// Indica se presente almeno una segnalazione di errore
        /// </summary>
        bool HaSegnalazioniErrore { get; }

        /// <summary>
        /// Indica se presente almeno una segnalazione di warning
        /// </summary>
        bool HaSegnalazioniWarning { get; }

    }
}