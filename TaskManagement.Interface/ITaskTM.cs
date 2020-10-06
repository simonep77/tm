using System;
using TaskManagement.Common;

namespace TaskInterfaceLib
{

    /// <summary>
    /// Eventi relativi al task
    /// </summary>
    public class TaskEvents
    {
        public delegate void OnTaskEventHandler(ITaskTM task);

        public delegate void OnTaskErrorEventHandler(ITaskTM task, Exception e);

        public delegate void OnProgressEventHandler(ITaskTM task, int item, int total);

        public delegate void OnTaskSegnalazione(ITaskTM task, ETipoSegnalazione tipo, string chiaveEntita, string tipoEntita, string testo);

    }

    /// <summary>
    /// Interfaccia base per la definizione di un task
    /// </summary>
    /// <remarks></remarks>
    /// 
    public interface ITaskTM: IDisposable
    {      
        event TaskEvents.OnProgressEventHandler OnReportProgress;
        event TaskEvents.OnTaskSegnalazione OnReportSegnalazione;

        /// <summary>
        /// Info di esecuzione
        /// </summary>
        TaskRuntimeinfo Runtime { get; }

        /// <summary>
        /// Esecuzione del task
        /// </summary>
        /// <returns></returns>
        void Execute();

    }
}