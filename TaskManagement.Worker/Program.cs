using Bdo.Objects;
using System;
using System.Diagnostics;
using System.IO;
using TaskInterfaceLib;
using TaskManagement.BIZ.src;
using TaskManagement.Worker.Properties;

namespace TaskManagement.Worker
{
    public class Applicazione
    {
        private static string TaskMainArg = string.Empty;

        [STAThread()]
        public static void Main(string[] args)
        {
            Console.WriteLine("*********************************************");
            Console.WriteLine(" TASKWORKER - Applicazione esecuzione task");
            Console.WriteLine("*********************************************");
            try
            {
                if (args.Length == 0)
                    throw new ArgumentException(@"Nessun parametro di avvio specificato");

                TaskMainArg = args[0];
                long lSchedPlanId = 0L;

                var slot = new BusinessSlot(@"TmConnection");
                var taskSvc = new TaskService(slot);


                if (long.TryParse(TaskMainArg, out lSchedPlanId))
                {
                    throw new NotImplementedException(@"Schedulazione non ancora implementata");
                    //Poi lo implemento
                }
                else
                {
                    taskSvc.RunTaskByName(TaskMainArg);
                }

                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                printUsage();
                printException(ex);

                Environment.ExitCode = 1;
            }
        }


        private static void printException(Exception ex)
        {
            var proc = Process.GetCurrentProcess();
            var logDir = string.IsNullOrWhiteSpace(Settings.Default.LogBaseDirectory) ? Path.GetTempPath() : Settings.Default.LogBaseDirectory;

            Directory.CreateDirectory(logDir);

            using (var oLog = new FileLogger(Path.Combine(logDir, $"{proc.ProcessName}_Errors_{DateTime.Now:yyyy_MM}.log")))
            {
                oLog.WriteLog("===================================================================================================");
                oLog.WriteLog("TaskWorker PID: {0}", (object)Process.GetCurrentProcess().Id);
                oLog.WriteLog("Nome Task     : {0}", TaskMainArg);
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