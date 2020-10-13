using Bdo.Objects;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TaskManagement.Common;
using TaskManagement.DAL;
using TaskManagement.Interface;

namespace TaskManagement.BIZ.src
{
    public class TaskDefinizioneBiz:BusinessObject<TaskDefinizione>
    {
        private Process mThisProcess = Process.GetCurrentProcess();
        private TaskEsecuzione mEsecuzione;

        /// <summary>
        /// Ottiene o imposta l'id del job di riferimento che ha originato l'esecuzione richiesta e che verra' registrato nell'esecuzione corrente
        /// </summary>
        public long ParentJobEsecuzioneId { get; set; }
        
        /// <summary>
        /// Ottiene o imposta il piano di schedulazione che ha originato l'esecuzione corrente
        /// </summary>
        public long PianoSchedulazioneId { get; set; }
        public TaskEsecuzione UltimaEsecuzione { get { return this.mEsecuzione; } }
        public Lazy<TaskParametroLista> Parametri { get; }
        public Lazy<TaskDettaglioJobLista> DettagliJob { get; }

        public TaskDefinizioneBiz(TaskDefinizione t) : base(t) {
            //Definizione Lazy load
            this.Parametri = new Lazy<TaskParametroLista>(() => this.Slot.CreateList<TaskParametroLista>().SearchByColumn(Filter.Eq(nameof(TaskParametro.TaskDefId), this.DataObj.Id)));
            this.DettagliJob = new Lazy<TaskDettaglioJobLista>(() => this.Slot.CreateList<TaskDettaglioJobLista>()
                                                                                .OrderBy(nameof(TaskDettaglioJob.Progressivo), OrderVersus.Asc)
                                                                                .SearchByColumn(Filter.Eq(nameof(TaskDettaglioJob.JobTaskDefId), this.DataObj.Id)));
        }



        private string getLogFileNameBase()
        {
            return $"{this.DataObj.Sistema.Nome.Replace(' ', '_')}_{this.DataObj.Nome.Replace(' ', '_')}";
        }


        /// <summary>
        /// Registra avvio esecuzione
        /// </summary>
        private void esecuzioneInizializzaTask(ITaskTM task)
        {
            task.Runtime.SysTaskClass = this.DataObj.TaskClass;
            task.Runtime.SysTaskId = this.DataObj.Id;
            task.Runtime.SysTaskType = this.DataObj.TipoTask.Nome;
            task.Runtime.SysAssemblyPath = this.DataObj.AssemblyPath;
            task.Runtime.SysDatiDir = this.DataObj.DatiDir;
            task.Runtime.SysLogDir = this.DataObj.LogDir;
            task.Runtime.SysSistema = this.DataObj.Sistema.Nome;
            task.Runtime.SysTipoNotifica = this.DataObj.TipoNotificaId;
            task.Runtime.SysRiferimento = this.DataObj.Riferimento;
            task.Runtime.SysNote = this.DataObj.Note;
            task.Runtime.TaskPID = this.mThisProcess.Id;

            task.Runtime.LogFileName = Path.Combine(this.DataObj.LogDir, $"{this.getLogFileNameBase()}_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            
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
                task.Runtime.UserParams.Add(item.Chiave, new Interface.TaskRuntimeParametro(item.Chiave,item.Valore, item.Visibile));
            }

            task.Runtime.TaskStartDate = this.mEsecuzione.DataInserimento;

            //Esegue le azioni propedeutiche all'avvio (creazione dirs)
            Directory.CreateDirectory(this.DataObj.LogDir);
            Directory.CreateDirectory(this.DataObj.DatiDir);


            //Se necessario imposta la visibilita' della console
            // if (!this.DataObj.MostraConsole)
            if (this.ParentJobEsecuzioneId == 0L)
            {
                Console.Title = $"{this.DataObj.Sistema.Nome} - {this.DataObj.Nome} (PID: {this.mThisProcess.Id})";
                ConsoleHelper.SetVisible(Console.Title, this.DataObj.MostraConsole);
            }

            //Inizializza
            task.Init();

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
            this.mEsecuzione.Host = Environment.MachineName;
            this.mEsecuzione.JobEsecuzioneId = this.ParentJobEsecuzioneId;//Eventuale dipendenza da job in esecuzione
            this.mEsecuzione.SchedPianoId = this.PianoSchedulazioneId;//Eventuale dipendenza da job in esecuzione
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


        private void esecuzioneEseguiNotifica(ITaskTM task)
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

                //Aggiunge log corrente
                var sbLogs = new StringBuilder(task.Runtime.LogFileName);

                //Se il task e' un job deve raccogliere i vari file di log
                if (task is TaskJob)
                {
                    var tJob = (TaskJob)task;
                    foreach (var item in tJob.TaskEseguiti)
                    {
                        if (!item.Eseguito)
                            continue;

                        var tExecBiz = item.Esito.ToBizObject<TaskEsecuzioneBiz>();

                        if (tExecBiz.Files.Value.Count > 0)
                        {
                            var sFile = Path.ChangeExtension(Path.Combine(item.Esito.Task.LogDir, tExecBiz.Files.Value.First().FileName), @".zip");

                            if (File.Exists(sFile))
                                sbLogs.Append($";{sFile}");
                        }

                    }
                }

                //Invia
                Mailer.Send(this.DataObj.MailFROM, this.DataObj.MailTO, this.DataObj.MailCC, this.DataObj.MailBCC, subj, body, sbLogs.ToString());

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

            var zip = new FastZip();
            zip.CreateZip(zipfile, Path.GetDirectoryName(logFile), false, Path.GetFileName(logFile));
            
            //Elimina file log non zippato
            File.Delete(logFile);


        }


        private void esecuzionePuliziaFileDB()
        {
            if (this.DataObj.MantieniNumLogDB <= 0)
                return;

            var lstFiles = this.Slot.CreateList<TaskFileLista>()
                .CercaFilesPerTaskDefId(this.DataObj.Id)
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

            var lstFiles = Directory.GetFiles(this.DataObj.LogDir, this.getLogFileNameBase() + "*")
                .OrderByDescending(f => new FileInfo(f).CreationTime).Skip(this.DataObj.MantieniNumLogFS);

            foreach (var f in lstFiles)
            {
                File.Delete(f);
            }
        }


        private Tuple<ITaskTM, AppDomain> esecuzioneIstanziaTipoTask()
        {
            switch (this.DataObj.TipoTaskId)
            {
                case (int)ETipoTask.TaskClass:
                    //Crea nuovo dominio di esecuzione
                    var domSetup = new AppDomainSetup();
                    domSetup.ApplicationBase = Path.GetDirectoryName(this.DataObj.AssemblyPath);
                    domSetup.ApplicationName = this.DataObj.TaskClass;
                    domSetup.PrivateBinPath = domSetup.ApplicationBase;
                    domSetup.ShadowCopyFiles = @"true";
                    //domSetup.ShadowCopyDirectories = "";

                    var appDom = AppDomain.CreateDomain(@"tmDomain", null, domSetup);
                    var task = (ITaskTM)appDom.CreateInstanceAndUnwrap(Path.GetFileNameWithoutExtension(this.DataObj.AssemblyPath), this.DataObj.TaskClass);
                    return new Tuple<ITaskTM, AppDomain>(task, appDom);

                case (int)ETipoTask.TaskExecPgm:
                    return new Tuple<ITaskTM, AppDomain>(new TaskExecProgram(), null);
                case (int)ETipoTask.TaskJob:
                    return new Tuple<ITaskTM, AppDomain>(new TaskJob(this), null);
                default:
                    throw new NotImplementedException($"tipo di task {this.DataObj.TipoTaskId} non implementato");
            }
        }


        public void Run()
        {
            var retCode = 0;
            var retMsg = string.Empty;
            ITaskTM task = null;
            AppDomain domain = null;

            try
            {
                //Crea istanza nell'altro dominio
                var tupla = this.esecuzioneIstanziaTipoTask();
                task = tupla.Item1;
                domain = tupla.Item2;

                //Crea avvio esecuzione DB
                this.esecuzioneAvvia();

                //Inizializza
                this.esecuzioneInizializzaTask(task);

                //Aggancia metodi base per tracciamento

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
                this.esecuzioneEseguiNotifica(task);

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
                if(domain != null)
                    AppDomain.Unload(domain);
            }

        }


     

    }
}
