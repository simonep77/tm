using Bdo.Objects;
using Bdo.Utils;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TaskInterfaceLib;
using TaskManagement.Common;
using TaskManagement.DAL;

namespace TaskManagement.BIZ.src
{
    public class TaskDefinizioneBiz:BusinessObject<TaskDefinizione>
    {
        private TaskEsecuzione mEsecuzione;
        public Lazy<TaskParametroLista> Parametri { get; }

        public TaskDefinizioneBiz(TaskDefinizione t) : base(t) {
            //Definizione Lazy load
            this.Parametri = new Lazy<TaskParametroLista>(() => this.Slot.CreateList<TaskParametroLista>().SearchByColumn(Filter.Eq(nameof(TaskParametro.TaskDefId), this.DataObj.Id)));
        }



        /// <summary>
        /// Registra avvio esecuzione
        /// </summary>
        private void esecuzioneInizializza(ITaskTM task)
        {
            task.Runtime.SysTaskClass = this.DataObj.TaskClass;
            task.Runtime.SysTaskId = this.DataObj.Id;
            task.Runtime.SysAssemblyPath = this.DataObj.AssemblyPath;
            task.Runtime.SysDatiDir = this.DataObj.DatiDir;
            task.Runtime.SysLogDir = this.DataObj.LogDir;
            task.Runtime.SysSistema = this.DataObj.Sistema.Nome;
            task.Runtime.SysTipoNotifica = this.DataObj.TipoNotificaId;
            task.Runtime.SysRiferimento = this.DataObj.Riferimento;
            task.Runtime.SysNote = this.DataObj.Note;

            task.Runtime.LogFileName = Path.Combine(this.DataObj.LogDir, $"{this.DataObj.Sistema.Nome.Replace(' ', '_')}_{this.DataObj.Nome.Replace(' ', '_')}_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            
            if (this.DataObj.TipoNotificaId == (int)ETipoNotificaEsito.Email)
            {
                task.Runtime.SysMailFROM = this.DataObj.TaskClass;
                task.Runtime.SysMailTO = this.DataObj.TaskClass;
                task.Runtime.SysMailCC = this.DataObj.TaskClass;
                task.Runtime.SysMailBCC = this.DataObj.TaskClass;
            }

            //Parametri
            foreach (var item in this.Parametri.Value)
            {
                task.Runtime.UserParams.Add(item.Chiave, new ParametroItem(item.Chiave,item.Valore, item.Visibile));
            }

            task.Runtime.TaskStartDate = this.mEsecuzione.DataInserimento;
            task.Runtime.InitLogger();
        }

        /// <summary>
        /// Registra avvio esecuzione
        /// </summary>
        private void esecuzioneAvvia()
        {
            this.mEsecuzione = this.Slot.CreateObject<TaskEsecuzione>();
            this.mEsecuzione.TaskDefId = this.DataObj.Id;
            this.mEsecuzione.StatoEsecuzioneId = (short)EStatoEsecuzione.InEsecuzione;
            this.mEsecuzione.Pid = Process.GetCurrentProcess().Id.ToString();
            this.Slot.SaveObject(this.mEsecuzione);
        }

        /// <summary>
        /// Registra avvio esecuzione
        /// </summary>
        private void esecuzioneRegistraFine(int code, string message)
        {
            this.mEsecuzione.DataTermine = DateTime.Now;
            this.mEsecuzione.StatoEsecuzioneId = (short)EStatoEsecuzione.Terminato;
            this.mEsecuzione.ReturnCode = code;
            this.mEsecuzione.ReturnMessage = message;
            this.Slot.SaveObject(this.mEsecuzione);
        }


        private void esecuzioneEseguiNotifica(string logFile)
        {
            if (this.DataObj.TipoNotificaId == (int)ETipoNotificaEsito.Nessuna)
                return;

            //Invia email
            try
            {
                ETaskReturnCode rc = (ETaskReturnCode)this.mEsecuzione.ReturnCode;
                var rctext = rc.ToString().ToUpper();

                var subj = $"{rctext} - {this.DataObj.Sistema.Nome} {this.DataObj.Nome} ";
                var body = $"Elaborazione avviata alle { this.mEsecuzione.DataInserimento:dd/MM/yyyy HH:mm:ss} e conclusa alle {this.mEsecuzione.DataTermine:dd/MM/yyyy HH:mm:ss}";

                Mailer.Send(this.DataObj.MailFROM, this.DataObj.MailTO, this.DataObj.MailCC, this.DataObj.MailBCC, subj, body, logFile);
            }
            catch (Exception e)
            {
                this.mEsecuzione.NotificaCode = -1;
                this.mEsecuzione.NotificaMessage = e.Message;
            }
            finally
            {
                this.mEsecuzione.ReturnCode = 0;
                this.mEsecuzione.ReturnMessage = "OK"; //A chi?
            }

            this.Slot.SaveObject(this.mEsecuzione);
        }


        private void esecuzioneCaricaLogFile(string logFile)
        {
            var log = this.Slot.CreateObject<TaskFile>();
            log.TaskEsecuzioneId = this.mEsecuzione.Id;
            log.TipoFileId = 1;
            log.FileName = Path.GetFileName(logFile);
            log.FileData = File.ReadAllBytes(logFile);

            this.Slot.SaveObject(log);
        }


        private void esecuzioneZipLogFilAndDelete(string logFile)
        {
            var zipfile = Path.ChangeExtension(logFile, @".zip");

            var zip = new ZipFile(zipfile);
            var fi = new FileInfo(logFile);
            var newEntry = new ZipEntry(ZipEntry.CleanName(fi.Name));
            newEntry.DateTime = fi.LastWriteTime;
            newEntry.Size = fi.Length;
            zip.Add(newEntry);
            zip.CommitUpdate();
            zip.Close();

            //Elimina file log non zippato
            File.Delete(logFile);


            //zip.Add(task.Runtime.Logger.Path, CompressionMethod.BZip2, )

            //using (var fsOut = File.Create(zipfile))
            //{
            //    using (var zipStream = new ZipOutputStream(fsOut))
            //    {
            //        zipStream.SetLevel(9);       // 0-9, 9 being the highest level of compression
            //        var fi = new FileInfo(task.Runtime.Logger.Path);
            //        var newEntry = new ZipEntry(ZipEntry.CleanName(fi.Name));
            //        newEntry.DateTime = fi.LastWriteTime;
            //        newEntry.Size = fi.Length;
            //        zipStream.PutNextEntry(newEntry);
            //        try
            //        {
            //            using (var streamReader = File.OpenRead(inputFile))
            //            {
            //                CopiaStream(streamReader, zipStream);
            //            }
            //        }
            //        finally
            //        {
            //            zipStream.CloseEntry();
            //        }

            //        zipStream.IsStreamOwner = true;
            //    }
            //}
        }


        private void esecuzionePuliziaFileDB()
        {
            if (this.DataObj.MantieniNumLogDB <= 0)
                return;

            var lstFiles = this.Slot.CreateList<TaskFileLista>()
                .OrderBy(nameof(TaskFile.DataInserimento))
                .SearchByColumn(Filter.Eq(nameof(TaskFile.TaskEsecuzioneId), this.DataObj.Id))
                .Skip(this.DataObj.MantieniNumLogDB);

            foreach (var f in lstFiles)
            {
                this.Slot.DeleteObject(f);
            }
        }


        private void esecuzionePuliziaFileFS()
        {
            if (this.DataObj.MantieniNumLogFS <= 0)
                return;

            var lstFiles = Directory.GetFiles(this.DataObj.LogDir).OrderBy(f => new FileInfo(f).CreationTime).Skip(this.DataObj.MantieniNumLogFS);

            foreach (var f in lstFiles)
            {
                File.Delete(f);
            }
        }


        
        public void Run()
        {
            var retCode = 0;
            var retMsg = string.Empty;
            //Crea info di dominio
            var domSetup = new AppDomainSetup();
            domSetup.ShadowCopyFiles = @"true";
            domSetup.ApplicationBase = Path.GetDirectoryName(this.DataObj.AssemblyPath);
            domSetup.ApplicationName = this.DataObj.TaskClass;
            domSetup.PrivateBinPath = domSetup.ApplicationBase;
            //domSetup.ShadowCopyDirectories = "";

            var appDom = AppDomain.CreateDomain(@"tmDomain", null, domSetup);
            try
            {
                //Crea istanza nell'altro dominio
                var task = (ITaskTM)appDom.CreateInstanceFromAndUnwrap(this.DataObj.AssemblyPath, this.DataObj.TaskClass);

                //Crea avvio esecuzione DB
                this.esecuzioneAvvia();

                //Inizializza
                this.esecuzioneInizializza(task);

                //Aggancia metodi base per tracciamento
                task.OnReportSegnalazione += SalvaSegnalazioneTask;

                try
                {
                    //Deve inizializzare i parametri di runtime del task
                    //Esegue codice Task
                    task.Execute();
                    //Imposta output
                    retCode = (int)task.Runtime.TaskLastReturnCode;
                    retMsg = task.Runtime.TaskLastMessage;
                }
                catch (Exception e)
                {
                    retCode = -99;
                    retMsg = e.ToString();
                }
                finally
                {
                    this.esecuzioneRegistraFine(retCode, retMsg);
                }

                //Esegue notifica email
                this.esecuzioneEseguiNotifica(task.Runtime.LogFileName);

                //Carica log su DB
                this.esecuzioneCaricaLogFile(task.Runtime.LogFileName);

                //Zip log su File System
                this.esecuzioneZipLogFilAndDelete(task.Runtime.LogFileName);

                //Esegue pulizia log db
                this.esecuzionePuliziaFileDB();

                //Esegue pulizia log su File System
                this.esecuzionePuliziaFileFS();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                AppDomain.Unload(appDom);
            }

        }

        private void SalvaSegnalazioneTask(ITaskTM task, ETipoSegnalazione cond, string itemKey, string itemType, string text)
        {
            throw new NotImplementedException();
        }

     

    }
}
