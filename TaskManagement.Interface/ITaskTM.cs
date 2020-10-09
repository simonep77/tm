using System;
using TaskManagement.Common;

namespace TaskManagement.Interface
{


    /// <summary>
    /// Interfaccia base per la definizione di un task
    /// </summary>
    /// <remarks></remarks>
    /// 
    public interface ITaskTM: IDisposable
    {      
        /// <summary>
        /// Info di esecuzione
        /// </summary>
        ITaskRunTimeInfo Runtime { get; }

        /// <summary>
        /// Richiama routine di inizializzazione
        /// </summary>
        void Init();

        /// <summary>
        /// Esecuzione del task
        /// </summary>
        /// <returns></returns>
        void Execute();

    }
}