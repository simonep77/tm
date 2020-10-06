﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManagement.Common;

namespace TaskInterfaceLib
{
    public abstract class TaskBase : MarshalByRefObject, ITaskTM
    {
        protected const string STR_LOG_SEP = @"===================================================================================";

        public TaskRuntimeinfo Runtime { get; } = new TaskRuntimeinfo();

        public event TaskEvents.OnProgressEventHandler OnReportProgress;
        public event TaskEvents.OnTaskSegnalazione OnReportSegnalazione;


        protected abstract bool CheckUserParams();
        protected abstract int TaskExecute();

        public void Dispose()
        {
            this.Runtime.Logger?.StopLog();
        }

        public void Execute()
        {
            this.Runtime.Logger.StartLog();
            try
            {
                //Prevalorizzazioni varie..
                this.Runtime.TaskLastReturnCode = (int)ETaskReturnCode.OK;

                //Scrive inizio
                this.printStartInfo();

                try
                {
                    //Verifica i parametri in ingresso
                    if (!this.CheckUserParams())
                        throw new ArgumentException(@"Verifica dei parametri in input non superata");

                    //Esegue
                    this.Runtime.TaskLastReturnCode = this.TaskExecute();

                }
                catch (Exception e)
                {
                    this.Runtime.TaskLastReturnCode = (int)ETaskReturnCode.ErroreGenerico;
                    this.Runtime.TaskLastMessage = e.Message;
                    this.Runtime.TaskEndDate = DateTime.Now;
                    this.WriteLog(e.Message);
                    throw;
                }
                finally
                {
                    //Imposta dati di fine
                    this.Runtime.TaskEndDate = DateTime.Now;
                }

                //Scrive segnalazioni
                this.printSegnalazioni();

                //Scrive fine task
                this.printEndInfo();
            }
            finally
            {
                this.Runtime.Logger.StopLog();
            }
            
        }

        #region PUBLIC

        /// <summary>
        /// Invia email
        /// </summary>
        /// <param name="mailfrom"></param>
        /// <param name="mailto"></param>
        /// <param name="mailcc"></param>
        /// <param name="mailbcc"></param>
        /// <param name="subj"></param>
        /// <param name="body"></param>
        /// <param name="attachs"></param>
        /// <remarks></remarks>
        protected void SendMail(string mailfrom, string mailto, string mailcc, string mailbcc, string subj, string body, string attachs)
        {
            var ATT_SEPS = new char[] { ',', ';' };
            var ATT_MAIL_SEPS = new char[] { ',', ';', ' ' };
            var oSmtp = new System.Net.Mail.SmtpClient();
            try
            {
                using (var oMsg = new System.Net.Mail.MailMessage())
                {

                    // Imposta dati
                    oMsg.IsBodyHtml = true;
                    oMsg.Subject = subj;
                    oMsg.Body = body;

                    // Indirizzi
                    if (!string.IsNullOrEmpty(mailfrom))
                    {
                        oMsg.From = new System.Net.Mail.MailAddress(mailfrom);
                    }

                    if (!string.IsNullOrEmpty(mailto))
                    {
                        foreach (string sEmail in mailto.Split(ATT_MAIL_SEPS, StringSplitOptions.RemoveEmptyEntries))
                            oMsg.To.Add(new System.Net.Mail.MailAddress(sEmail));
                    }

                    if (!string.IsNullOrEmpty(mailcc))
                    {
                        foreach (string sEmail in mailcc.Split(ATT_MAIL_SEPS, StringSplitOptions.RemoveEmptyEntries))
                            oMsg.CC.Add(new System.Net.Mail.MailAddress(sEmail));
                    }

                    if (!string.IsNullOrEmpty(mailbcc))
                    {
                        foreach (string sEmail in mailbcc.Split(ATT_MAIL_SEPS, StringSplitOptions.RemoveEmptyEntries))
                            oMsg.Bcc.Add(new System.Net.Mail.MailAddress(sEmail));
                    }

                    // Logga Operazione (prima di bloccare il file di log con l'attachment)
                    WriteLog(" Dati Invio Email");
                    WriteLog(" * Host       : {0}", oSmtp.Host);
                    WriteLog(" * Porta      : {0}", (object)oSmtp.Port);
                    WriteLog(" * Mittente   : {0}", oMsg.From.Address);
                    WriteLog(" * Destinatari: {0}", mailto);
                    WriteLog(" * CC         : {0}", mailcc);
                    WriteLog(" * BCC        : {0}", mailbcc);
                    WriteLog("");

                    // Attachments
                    if (!string.IsNullOrEmpty(attachs))
                    {
                        foreach (string sFile in attachs.Split(ATT_SEPS, StringSplitOptions.RemoveEmptyEntries))
                            oMsg.Attachments.Add(new System.Net.Mail.Attachment(sFile));
                    }

                    // INVIA
                    oSmtp.Send(oMsg);
                }

                WriteLog(" Esito Invio  : OK");
            }
            catch (Exception ex)
            {
                WriteLog(" Esito Invio  : KO");
                WriteLog(" Motivo       : {0}", ex.Message);
                throw;
            }
        }


        #endregion

        #region PROTECTED

        /// <summary>
        /// Scrive log
        /// </summary>
        /// <param name="messageFmt"></param>
        /// <param name="args"></param>
        /// <remarks></remarks>
        protected void WriteLog(string messageFmt, params object[] args)
        {
            this.Runtime.Logger.WriteLog(messageFmt, args);
        }

        /// <summary>
        /// Segnala al sistema che si e' verificata una condizione da gestire (forse)
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="itemKey"></param>
        /// <param name="itemType"></param>
        /// <param name="text"></param>
        protected void ReportSegnalazione(ETipoSegnalazione tipo, string chiaveEntita, string tipoEntita, string testo)
        {
            this.Runtime.Segnalazioni.Add(new TaskSegnalazione() { Tipo = tipo, ChiaveEntita= chiaveEntita, TipoEntita= tipoEntita, Testo= testo });
            this.WriteLog($"Segnalazione {tipo} - {tipoEntita} - {chiaveEntita} - {testo}");
            this.OnReportSegnalazione?.Invoke(this, tipo, chiaveEntita, tipoEntita, testo);
        }

        /// <summary>
        /// Segnala l'avanzamento della procedura
        /// </summary>
        /// <param name="cond"></param>
        /// <param name="itemKey"></param>
        /// <param name="itemType"></param>
        /// <param name="text"></param>
        protected void ReportProgress(int item, int total)
        {
            WriteLog(" ** Avanzamento: {0} di {1}", item, total);
            this.OnReportProgress?.Invoke(this, item, total);
        }


        /// <summary>
        /// Ritorna valore di configurazione User o valore di default se non trovato o nullo
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected string GetUserParamWithDefault(string key, string defaultValue)
        {
            ParametroItem item = null;
            
            if (!this.Runtime.UserParams.TryGetValue(key, out item) || string.IsNullOrWhiteSpace(item.Valore))
                return defaultValue;

            return item.Valore;
        }

        #endregion

        #region PRIVATE


        private void printStartInfo()
        {
            const int I_PAD_LEN = 25;
            WriteLog(STR_LOG_SEP);
            WriteLog("INIZIO ESECUZIONE TASK {0}", this.Runtime.SysTaskName);
            WriteLog(string.Empty);
            WriteLog("Tipo Task   : {0}", "Task Class");
            WriteLog("Assembly    : {0}", this.Runtime.SysAssemblyPath);
            WriteLog("TaskClass   : {0}", this.Runtime.SysTaskClass);

           

            WriteLog("Sistema     : {0}", this.Runtime.SysSistema);
            WriteLog("Descrizione : {0}", this.Runtime.SysNote);
            WriteLog("Riferimento : {0}", this.Runtime.SysRiferimento);
            WriteLog("LogDir      : {0}", this.Runtime.SysLogDir);

            WriteLog("TipoNotifica: {0}", this.Runtime.SysTipoNotifica);
            if (this.Runtime.SysTipoNotifica == (int)ETipoNotificaEsito.Email)
            {
                WriteLog("MailFROM    : {0}", this.Runtime.SysMailFROM);
                WriteLog("MailTO      : {0}", this.Runtime.SysMailTO);
                WriteLog("MailCC      : {0}", this.Runtime.SysMailCC);
                WriteLog("MailBCC     : {0}", this.Runtime.SysMailBCC);
            }

            WriteLog(STR_LOG_SEP);
            WriteLog("DATI ESECUZIONE TASK");
            WriteLog(string.Empty);
            WriteLog("ProcessID   : {0}", this.Runtime.TaskPID);
            WriteLog("HostName    : {0}", Environment.MachineName);
            WriteLog("UserName    : {0}", Environment.UserName);
            WriteLog("DomainName  : {0}", Environment.UserDomainName);
            WriteLog("HostSystem  : {0}", Environment.OSVersion.VersionString);
            WriteLog(STR_LOG_SEP);


            // Param utente
            WriteLog("PARAMETRI UTENTE ({0})", this.Runtime.UserParams.Count);
            WriteLog(string.Empty);
            foreach (var item in this.Runtime.UserParams.Values)
            {
                if (item.IsVisibile)
                    WriteLog("  * {0} => {1}", item.Chiave.PadRight(I_PAD_LEN), item.Valore);
                else
                    WriteLog("  * {0} => {1}", item.Chiave.PadRight(I_PAD_LEN), "Non Visualizzato");
            }

            WriteLog(STR_LOG_SEP);
        }


        private void printSegnalazioni()
        {
            WriteLog(STR_LOG_SEP);
            WriteLog("***** SEGNALAZIONI DEL TASK *****");
            WriteLog(string.Empty);

            var segGrouped = this.Runtime.Segnalazioni.GroupBy(s => s.Tipo).OrderByDescending(g => g.Key);

            foreach (var item in segGrouped)
            {
                WriteLog($"{item.Key.ToString().ToUpper().PadRight(12, ' ')}: {item.Count()}");

                foreach (var seg in item)
                {
                    this.WriteLog($"  > {seg.Tipo} - {seg.TipoEntita} - {seg.ChiaveEntita} - {seg.Testo}");
                }
            WriteLog(string.Empty);
            WriteLog(string.Empty);
            }


        }

        private void printEndInfo()
        {
            WriteLog(STR_LOG_SEP);
            WriteLog("INFO FINE ESECUZIONE TASK");
            WriteLog(string.Empty);
            WriteLog("Inizio: {0}", this.Runtime.TaskStartDate.ToString("dd/MM/yyyy HH:mm:ss"));
            WriteLog("Fine  : {0}", this.Runtime.TaskEndDate.ToString("dd/MM/yyyy HH:mm:ss"));
            WriteLog("Durata: {0} (ore:min:sec.msec)", this.Runtime.TaskEndDate.Subtract(this.Runtime.TaskStartDate).ToString());
            WriteLog("Esito : {0}", this.Runtime.TaskLastReturnCode);
        }


        #endregion
    }
}
