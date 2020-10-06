using System;
using System.Diagnostics;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using TaskInterface;
using TaskInterfaceLib;
using TaskManagement.BIZ.src;

namespace TaskWorker
{
    public class Applicazione
    {
        [STAThread()]
        public static void Main(string[] args)
        {
            Console.WriteLine("*********************************************");
            Console.WriteLine(" TASKWORKER - Applicazione esecuzione task");
            Console.WriteLine("*********************************************");
            try
            {
                string sTaskName = string.Empty;
                int iTaskSchedId = 0;
                if (args.Length == 0)
                    throw new ArgumentException(@"Nessun parametro di avvio specificato");

                var taskArg = args[0];
                long lSchedPlanId = 0L;

                var taskSvc = new TaskService();


                if (long.TryParse(taskArg, out lSchedPlanId))
                {
                    throw new NotImplementedException(@"Schedulazione non ancora implementata");
                    //Poi lo implemento
                }
                else
                {
  
                }




                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                printUsage();
                using (var oLog = new FileLogger(Path.Combine(My.MySettingsProperty.Settings.LogBaseDirectory, string.Concat("TaskWorker_Errors_", DateTime.Now.ToString("yyyy_MM_dd"), "_pid", Process.GetCurrentProcess().Id, ".log"))))
                {
                    oLog.WriteLog("===================================================================================================");
                    oLog.WriteLog("TaskWorker PID: {0}", (object)Process.GetCurrentProcess().Id);
                    oLog.WriteLog("Nome Task     : {0}", args[0]);
                    Console.WriteLine("Esecuzione terminata con errori.");
                    oLog.WriteLog("Esecuzione terminata con errori.");
                    Console.WriteLine(ex.Message);
                    oLog.WriteLog(ex.Message);
                }

                Environment.ExitCode = 1;
            }
        }

        private class TmArgs
        {
            /// <summary>
            /// Identificativo del task da eseguire
            /// </summary>
            public string TaskName { get; set; }
            
            /// <summary>
            /// Identificativo della schedulazione pianificata da eseguire
            /// </summary>
            public string ScPlanId { get; set; }
        }

        /// <summary>
    /// Visualizza messaggio
    /// </summary>
    /// <remarks></remarks>
        private static void printUsage()
        {
            Console.WriteLine(typeof(Applicazione).Assembly.GetName().Name);
            Console.WriteLine("<nometask> -> Deve contenre almeno un carattere non numerico -> esegue il task in modalità standalone");
            Console.WriteLine("oppure");
            Console.WriteLine("<schedule_plan_id> -> Solo numerico -> Esegue il task relativo alla schedulazione indicata");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("");
        }



    }
}