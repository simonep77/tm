using Bdo.Objects;
using System;
using System.Diagnostics;
using System.IO;
using TaskManagement.Common;
using TaskManagement.BIZ.src;
using TaskManagement.Worker.Properties;

namespace TaskManagement.Worker
{
    public class Applicazione
    {
        private static string taskMainArg = string.Empty;
        private static Process thisProc = Process.GetCurrentProcess();

        [STAThread()]
        public static void Main(string[] args)
        {
            Console.WriteLine("*********************************************");
            Console.WriteLine($" {thisProc.ProcessName} - Applicazione esecuzione task");
            Console.WriteLine("*********************************************");
            try
            {
                if (args.Length == 0)
                    throw new ArgumentException(@"Nessun parametro di avvio specificato");

                taskMainArg = args[0];
                long lSchedPlanId = 0L;

                var slot = new BusinessSlot(@"TmConnection");
                var taskSvc = new TaskService(slot);


                if (long.TryParse(taskMainArg, out lSchedPlanId))
                {
                    
                    //taskSvc.RebuildPlanByTaskId(3, 2);
                    taskSvc.RunTaskByPianoSchedId(lSchedPlanId);
                }
                else
                {
                    taskSvc.RunTaskByName(taskMainArg);
                }

                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                printUsage();

                while (ex != null)
                {
                    printException(ex);
                    ex = ex.InnerException;
                }


                Environment.ExitCode = 1;
            }
        }


        private static void printException(Exception ex)
        {
            var logDir = string.IsNullOrWhiteSpace(Settings.Default.LogBaseDirectory) ? Path.GetTempPath() : Settings.Default.LogBaseDirectory;

            Directory.CreateDirectory(logDir);

            using (var oLog = new FileLogger(Path.Combine(logDir, $"{thisProc.ProcessName}_Errors_{DateTime.Now:yyyy_MM}.log")))
            {
                oLog.WriteLog("===================================================================================================");
                oLog.WriteLog($"{thisProc.ProcessName} PID: {thisProc.Id}");
                oLog.WriteLog($"Nome Task     : {taskMainArg}");
                oLog.WriteLog("Esecuzione terminata con errori.");
                oLog.WriteLog(ex.Message);
            }

        }

        /// <summary>
        /// Visualizza messaggio
        /// </summary>
        /// <remarks></remarks>
        private static void printUsage()
        {
            Console.WriteLine(thisProc.ProcessName);
            Console.WriteLine("<nometask> -> Deve contenre almeno un carattere non numerico -> esegue il task in modalità standalone");
            Console.WriteLine("oppure");
            Console.WriteLine("<schedule_plan_id> -> Solo numerico -> Esegue il task relativo alla schedulazione indicata");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("");
        }



    }
}