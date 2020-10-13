using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TaskManagement.Common;
using TaskManagement.DAL;
using TaskManagement.Interface;

namespace TaskManagement.BIZ.src
{
    public class TaskJob : TaskBaseTM
    {

        public List<EsitoTaskEseguito> TaskEseguiti { get; } = new List<EsitoTaskEseguito>();

        public class EsitoTaskEseguito
        {
            public TaskDettaglioJob DettJob;
            public bool Eseguito;
            public TaskEsecuzione Esito;
        }

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

            //Print
            this.printAvvioJobs();

            foreach (var jobdett in this.mTaskJobBiz.DettagliJob.Value)
            {
                this.printJob(jobdett);

                if (jobdett.Attivo == 0)
                {
                    WriteLog(string.Empty);
                    WriteLog($"Attenzione! Il task {jobdett.Progressivo} - {jobdett.SubTask.Nome} non viene avviato in quanto NON ATTIVO a livello di JOB ");
                    //Aggiunge a task eseguiti
                    this.TaskEseguiti.Add(new EsitoTaskEseguito() { DettJob= jobdett, Eseguito=false, Esito = null });
                    //Il task successivo parte con il return code dell'ultimo in errore
                    continue;
                }

               //Se e' impostato un RC minimo non esegue
               if (iret >= jobdett.MinPredReturnCode)
                {
                    WriteLog(string.Empty);
                    WriteLog($"Attenzione! Il task {jobdett.Progressivo} - {jobdett.SubTask.Nome} non viene avviato per mancata verifica su returnCode del task precedente {iret} ");
                    //Aggiunge a task eseguiti
                    this.TaskEseguiti.Add(new EsitoTaskEseguito() { DettJob = jobdett, Eseguito = false, Esito = null });
                    //Il task successivo parte con il return code dell'ultimo in errore
                    continue;
                }

               //Se non richiesto l'invio email di esecuzione del task lo disattiva
               if (jobdett.AbilitaNotifiche == 0)
                    jobdett.SubTask.TipoNotificaId = (short)ETipoNotificaEsito.Nessuna;
                
               //Istanzia ed esegue
                var subTaskBiz = jobdett.SubTask.ToBizObject<TaskDefinizioneBiz>();
                //Imposta l'id di esecuzione del job principale
                subTaskBiz.ParentJobEsecuzioneId = this.mTaskJobBiz.UltimaEsecuzione.Id;
                //Esegue
                subTaskBiz.Run();
                //Imposta task eseguito
                this.TaskEseguiti.Add(new EsitoTaskEseguito() { DettJob= jobdett, Eseguito=true, Esito = subTaskBiz.UltimaEsecuzione });
                //Imposta ultima esecuzione
                iret = subTaskBiz.UltimaEsecuzione.ReturnCode;
                //Aggiorna errori globale
                iretGlobale += iret;
            }

            this.printFineJob();

            return iret;
        }


        private void printAvvioJobs()
        {
            // Print Task da eseguire
            WriteLog($"Task che compongono il job: {this.mTaskJobBiz.DettagliJob.Value.Count}");

            foreach (var jobdett in this.mTaskJobBiz.DettagliJob.Value)
            {
                WriteLog($"{jobdett.Progressivo.ToString().PadLeft(2,'0')} - {jobdett.SubTask.Nome.PadRight(40, ' ')} - Attivo: {jobdett.Attivo}");
            }

            WriteLog(string.Empty);
        }

        private void printJob(TaskDettaglioJob dett)
        {
            // Print Task da eseguire
            WriteLog(string.Empty);
            WriteLog(string.Empty);
            WriteLog(string.Empty);
            WriteLog(string.Empty.PadRight(STR_LOG_SEP.Length,'*'));
            WriteLog($" ({dett.Progressivo}) AVVIO {dett.SubTask.Nome.ToUpper()}");
            WriteLog(string.Empty.PadRight(STR_LOG_SEP.Length, '*'));
            WriteLog(string.Empty);
        }


        private void printFineJob()
        {
            // Print Task da eseguire
            WriteLog(string.Empty);
            WriteLog(string.Empty);
            WriteLog(string.Empty);
            WriteLog(string.Empty.PadRight(STR_LOG_SEP.Length,'*'));
            WriteLog($"*** ESITO DEI TASK DEL JOB: ");
            WriteLog(string.Empty.PadRight(STR_LOG_SEP.Length, '*'));

            foreach (var item in this.TaskEseguiti)
            {
                var sEseguito = string.Concat("- Eseguito: ", item.Eseguito ? "SI" : "NO");
                var sEsito = string.Empty;
                if (item.Eseguito)
                    sEsito  = string.Concat("- Esito: ", item.Esito.ReturnCode == 0 ? "OK" : "ERRORE");

                WriteLog($"***  ({item.DettJob.Progressivo.ToString(),2}) - ID# {item.DettJob.SubTaskDefId,3} {item.DettJob.SubTask.Nome,-40} {sEseguito} {sEsito}");
                WriteLog(string.Empty);

            }
            WriteLog(string.Empty.PadRight(STR_LOG_SEP.Length, '*'));

            WriteLog(string.Empty);
            WriteLog(string.Empty);
            WriteLog(string.Empty);

        }


    }
}
