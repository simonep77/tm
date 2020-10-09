using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TaskManagement.Common;

namespace TaskManagement.Interface
{
    /// <summary>
    /// Tipologia di task che esegue un programma esterno e ne redireziona output ed errori sul processo principale di esecuzione
    /// </summary>
    public class TaskExecProgram : TaskBaseTM
    {
        protected override bool CheckUserParams()
        {
            return true;
        }

        protected override int TaskExecute()
        {
            this.WriteLog($"Preparazione esecuzione di \"{Path.GetFileName(this.Runtime.SysAssemblyPath)}\" \"{this.Runtime.SysTaskClass}\"");

            var pInfo = new ProcessStartInfo();
            pInfo.FileName = this.Runtime.SysAssemblyPath;
            pInfo.WorkingDirectory = Path.GetDirectoryName(this.Runtime.SysAssemblyPath);
            pInfo.Arguments = this.Runtime.SysTaskClass;
            pInfo.UseShellExecute = false;
            pInfo.CreateNoWindow = true;
            pInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pInfo.RedirectStandardOutput = true;
            pInfo.RedirectStandardError = true;
            try
            {
                // Crea e avvia processo
                using (var p = new Process())
                {
                    p.StartInfo = pInfo;
                    p.OutputDataReceived += ReceivePgmOutput;
                    p.ErrorDataReceived += ReceivePgmOutput;
                    p.Start();

                    //Scrive PID lanciato
                    this.WriteLog($"Processo avviato con PID: {p.Id}");
                    this.WriteLog(string.Empty);

                    // Inizia a leggere output
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();

                    // Attende chiusura
                    p.WaitForExit();
                    return p.ExitCode;
                }
            }
            catch (Exception ex)
            {
                WriteLog("Errore esecuzione programma: {0}", ex.Message);
                return (int)ETaskReturnCode.ErroreGenerico;
            }
        }

        private void ReceivePgmOutput(object sender, DataReceivedEventArgs e)
        {
            if (e is null || e.Data is null)
            {
                return;
            }

            WriteLog(e.Data);
        }
    }

}
