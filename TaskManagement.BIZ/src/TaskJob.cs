using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManagement.Common;
using TaskManagement.DAL;
using TaskManagement.Interface;

namespace TaskManagement.BIZ.src
{
    public class TaskJob : TaskBaseTM
    {


        public TaskJob(TaskDefinizioneBiz tb)
        {
            this.mTaskJobBiz = tb;
        }

        protected override bool CheckUserParams()
        {
            return true;
        }

        private TaskDefinizioneBiz mTaskJobBiz;
        protected override int TaskExecute()
        {
            var iret = 0;
            var iretGlobale = 0;
            var svc = new TaskService(this.mTaskJobBiz.GetSlot());
            var lstTaskEseguiti = new List<TaskDefinizioneBiz>();
            var lstTaskNonEseguiti = new List<TaskDettaglioJob>();

            //Print
            this.preparaAvvioJob();

            foreach (var jobdett in this.mTaskJobBiz.DettagliJob.Value)
            {
                if (jobdett.Attivo == 0)
                {
                    WriteLog(string.Empty);
                    WriteLog($"Attenzione! Il task {jobdett.Progressivo} - {jobdett.SubTask.Nome} non viene avviato in quanto NON ATTIVO a livello di JOB ");
                    lstTaskNonEseguiti.Add(jobdett);
                    //Il task successivo parte con il return code dell'ultimo in errore
                    continue;
                }

               //Se e' impostato un RC minimo non esegue
               if (iret >= jobdett.MinPredReturnCode)
                {
                    WriteLog(string.Empty);
                    WriteLog($"Attenzione! Il task {jobdett.Progressivo} - {jobdett.SubTask.Nome} non viene avviato per mancata verifica su returnCode del task precedente {iret} ");
                    lstTaskNonEseguiti.Add(jobdett);
                    //Il task successivo parte con il return code dell'ultimo in errore
                    continue;
                }

               if (jobdett.AbilitaNotifiche == 0)
                jobdett.SubTask.TipoNotificaId = (short)ETipoNotificaEsito.Nessuna;
                
               //Istanzia ed esegue
                var subTaskBiz = jobdett.SubTask.ToBizObject<TaskDefinizioneBiz>();
                //Imposta l'id di esecuzione del job principale
                subTaskBiz.ParentJobEsecuzioneId = this.mTaskJobBiz.UltimaEsecuzione.Id;
                //Esegue
                subTaskBiz.Run();

                //Imposta ultima esecuzione
                iret = subTaskBiz.UltimaEsecuzione.ReturnCode;
                //Aggiorna errori globale
                iretGlobale += iret;
            }

            return iret;
        }


        private void preparaAvvioJob()
        {
            // Print Task da eseguire
            WriteLog($"Task che compongono il job: {this.mTaskJobBiz.DettagliJob.Value.Count}");

            foreach (var jobdett in this.mTaskJobBiz.DettagliJob.Value)
            {
                WriteLog($"{jobdett.Progressivo.ToString().PadLeft(2,'0')} - {jobdett.SubTask.Nome.PadRight(40, ' ')} - Attivo: {jobdett.Attivo}");
            }

            WriteLog(string.Empty);

        }





    }
}
