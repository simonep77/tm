using Bdo.Objects;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private TaskSchedulazionePiano mPianoSched;

        /// <summary>
        /// Ottiene o imposta l'id del job di riferimento che ha originato l'esecuzione richiesta e che verra' registrato nell'esecuzione corrente
        /// </summary>
        public long ParentJobEsecuzioneId { get; set; }


        /// <summary>
        /// indica se il task e' un Job
        /// </summary>
        public bool IsJob { 
            get
            {
                return (this.DataObj.TipoTaskId == (short)ETipoTask.TaskJob);
            } 
        }

        /// <summary>
        /// Indica se il task sta girando o ha girato come sotto-task di un job
        /// </summary>
        public bool IsRunningUnderJob
        {
            get
            {
                return (this.DataObj.TipoTaskId != (short)ETipoTask.TaskJob && this.ParentJobEsecuzioneId > 0L);
            }
        }

        /// <summary>
        /// Indica se sta girando in modalita' schedulata
        /// </summary>
        public bool IsRunningUnderSchedule
        {
            get
            {
                return (this.PianoSchedulazioneId > 0L);
            }
        }


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
            task.Runtime.TaskStartDate = this.mEsecuzione.DataInserimento;

            if (this.DataObj.TipoNotificaId == (int)ETipoNotificaEsito.Email)
            {
                task.Runtime.SysMailFROM = this.DataObj.MailFROM;
                task.Runtime.SysMailTO = this.DataObj.MailTO;
                task.Runtime.SysMailCC = this.DataObj.MailCC;
                task.Runtime.SysMailBCC = this.DataObj.MailBCC;
            }

            //Parametri validi solo su tipo task class
            if (this.DataObj.TipoTaskId == (short)ETipoTask.TaskClass)
            {
                foreach (var item in this.Parametri.Value)
                {
                    //Imposta di base i parametri specifici
                    var rtParam = new Interface.TaskRuntimeParametro();
                    rtParam.Chiave = item.Chiave;
                    rtParam.IsCondiviso = (item.IsCondiviso > 0);
                    rtParam.IsVisibile = item.Visibile;
                    rtParam.Valore = item.Valore;
                    rtParam.ValoreOpzionale = item.ValoreOpzionale;

                    if (rtParam.IsCondiviso)
                    {
                        //Carica parametro condiviso
                        var paramCondiviso = this.Slot.LoadObjByKEY<TaskParametroCondiviso>(TaskParametroCondiviso.KEY_CHIAVE, item.Chiave);
                        //Aggiunge ad elenco
                        rtParam.Valore = paramCondiviso.Valore;
                        rtParam.ValoreOpzionale = paramCondiviso.ValoreOpzionale;

                    }

                    task.Runtime.UserParams.Add(item.Chiave, rtParam);
                }

                //Qui in caso di esecuzione pianificata potrebbero essere passati parametri di override
                if (this.IsRunningUnderSchedule)
                {
                    if (!string.IsNullOrWhiteSpace(this.mPianoSched.JsonParametriOverride))
                    {
                        //TDO da implementare
                        var obj = JsonConvert.DeserializeObject(this.mPianoSched.JsonParametriOverride) as JObject;
                       
                        foreach (var item in obj)
                        {
                            //Imposta di base i parametri specifici
                            var rtParam = new Interface.TaskRuntimeParametro();
                            rtParam.Chiave = item.Key;
                            rtParam.IsVisibile = true;
                            rtParam.Valore = item.Value.ToString();
                            task.Runtime.UserParams[rtParam.Chiave] = rtParam;
                        }

                    }
                }

            }



            //Esegue le azioni propedeutiche all'avvio (creazione dirs)
            Directory.CreateDirectory(this.DataObj.LogDir);
            Directory.CreateDirectory(this.DataObj.DatiDir);


            //Se necessario imposta la visibilita' della console
            // if (!this.DataObj.MostraConsole)
            if (this.ParentJobEsecuzioneId == 0L)
            {
                Console.Title = $"{this.DataObj.Sistema.Nome} - {this.DataObj.Nome} (PID: {this.mThisProcess.Id})";
                ConsoleHelper.SetVisible(Console.Title, (this.DataObj.MostraConsole > 0));
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
            this.mEsecuzione.StatoEsecuzioneId = EStatoEsecuzione.InEsecuzione;
            this.mEsecuzione.Pid = Process.GetCurrentProcess().Id.ToString();
            this.mEsecuzione.Host = Environment.MachineName;
            this.mEsecuzione.JobEsecuzioneId = this.ParentJobEsecuzioneId;//Eventuale dipendenza da job in esecuzione
            this.mEsecuzione.SchedPianoId = this.PianoSchedulazioneId;//Eventuale dipendenza da job in esecuzione
            this.Slot.SaveObject(this.mEsecuzione);

            //Se schedulato comunque carica il piano di riferimento
            if (this.IsRunningUnderSchedule)
                this.mPianoSched = this.Slot.LoadObjByPK<TaskSchedulazionePiano>(this.PianoSchedulazioneId);


        }

        /// <summary>
        /// Registra avvio esecuzione
        /// </summary>
        private void esecuzioneRegistraFine(int code, string message)
        {
            this.mEsecuzione.DataTermine = DateTime.Now;
            this.mEsecuzione.StatoEsecuzioneId = EStatoEsecuzione.Terminato;
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

                //Se il task e' un job deve raccogliere i vari file di log e scrivere una mail piu' dettagliata
                if (this.IsJob)
                {
                    var sbBody = new StringBuilder(body);
                    sbBody.Append(@"<br /><ul>");

                    var tJob = (TaskJob)task;
                    foreach (var item in tJob.TaskEseguiti)
                    {
                        //Scrive output esito vari subtask
                        sbBody.Append($"<li> N. {item.DettJob.Progressivo} - {item.DettJob.SubTask.Id} {item.DettJob.SubTask.Nome}");
                        sbBody.Append($"<br/>Eseguito: {item.Eseguito} - Esito: {item.Esito?.ReturnCode} {item.Esito?.ReturnMessage}</li>");

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
                    sbBody.Append(@"</ul>");

                    body = sbBody.ToString();
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


        private class IstanzaTask
        {
            public ITaskTM Task;
            public AppDomain Domain;
        }

        private IstanzaTask esecuzioneIstanziaTipoTask()
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
                    return new IstanzaTask() { Task= task, Domain=appDom };

                case (int)ETipoTask.TaskExecPgm:
                    return new IstanzaTask() { Task = new TaskExecProgram(), Domain = null };
                case (int)ETipoTask.TaskJob:
                    return new IstanzaTask() { Task = new TaskJob(this), Domain = null };
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
                task = tupla.Task;
                domain = tupla.Domain;

                //Crea avvio esecuzione DB
                this.esecuzioneAvvia();

                //Inizializza
                this.esecuzioneInizializzaTask(task);

                try
                {
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
            catch (Exception e)
            {

                throw;
            }
            finally
            {
                if(domain != null)
                    AppDomain.Unload(domain);
            }

        }


        public TaskSchedulazionePiano CreaSchedulazione(DateTime dtWhen, bool manuale, bool alreadyRunning, string jsonParametri = null)
        {
            var plNew = this.Slot.CreateObject<TaskSchedulazionePiano>();
            plNew.TaskDefId = this.DataObj.Id;
            plNew.DataEsecuzione = dtWhen;
            plNew.IsManuale = Convert.ToSByte(manuale ? 1 : 0);
            plNew.StatoEsecuzioneId = (alreadyRunning ? EStatoEsecuzione.PS_InEsecuzione : EStatoEsecuzione.PS_Pianificato);
            plNew.JsonParametriOverride = jsonParametri;
            this.Slot.SaveObject(plNew);

            return plNew;
        }


        /// <summary>
        /// Ricostruisce il piano di schedulazione o restituisce il solo piano di competenza
        /// </summary>
        /// <param name="planDateEnd"></param>
        /// <param name="isMaster"></param>
        /// <param name="nodoId"></param>
        /// <returns></returns>
        public List<TaskSchedulazionePiano> ReBuildSchedulePlan(DateTime planDateEnd, bool isMaster, int nodoId)
        {

            var currPlan = this.Slot.CreateList<TaskSchedulazionePianoLista>()
                .LoadFullObjects()
                .SearchByColumn(Filter.Eq(nameof(TaskSchedulazionePiano.TaskDefId), this.DataObj.Id)
                .And(Filter.Eq(nameof(TaskSchedulazionePiano.StatoEsecuzioneId), EStatoEsecuzione.PS_Pianificato)));

            var dateStart = DateTime.Now;

            var newPlan = new List<TaskSchedulazionePiano>();

            //Verifichiamo la stringa cron di schedulazione e controlliamo le schedulazioni esistenti
            if (!string.IsNullOrWhiteSpace(this.DataObj.SchedCronString))
            {
                var cronExpr = NCrontab.Advanced.CrontabSchedule.Parse(this.DataObj.SchedCronString);


                var dates = cronExpr.GetNextOccurrences(dateStart, planDateEnd);

                //Se siamo in esecuzione slave ritorniamo solo le schedulazioni di competenza del nodo
                if (!isMaster)
                    return currPlan.Where(p => p.Task.SchedNodoId == nodoId).ToList();

                //Verifico esistenza match piano gia' creato che non verra' modificato
                foreach (var dt in dates)
                {
                    //Cerca schedulazione
                    var sched = currPlan.Where(d => d.DataEsecuzione == dt).FirstOrDefault();

                    if (sched != null)
                    {
                        //Rimuove da elenco
                        newPlan.Add(sched);
                        currPlan.Remove(sched);
                    }
                    else //Crea nuova schedulazione
                        newPlan.Add(this.CreaSchedulazione(dt, false, false));

                }
            }
            else
            {
                //Ritorna un piano vuoto
                if (currPlan.Count == 0)
                    return newPlan;
            }

            //Gestioamo eventuali pianificazioni rimaste
            if (currPlan.Count > 0)
            {
                //Marca come saltate le schedulazioni passate non avviate (sia manuali che automatiche)
                var pastOldPlan = currPlan.FindAllByPropertyFilter(Filter.Lte(nameof(TaskSchedulazionePiano.DataEsecuzione), dateStart));
                pastOldPlan.SetPropertyMassive(nameof(TaskSchedulazionePiano.StatoEsecuzioneId), EStatoEsecuzione.PS_Saltato);
                this.Slot.SaveAll(pastOldPlan);

                //Individua le future non piu' schedulate
                var nextOldPlan = currPlan.FindAllByPropertyFilter(Filter.Gt(nameof(TaskSchedulazionePiano.DataEsecuzione), dateStart).And(Filter.Eq(nameof(TaskSchedulazionePiano.IsManuale), 0)));
                this.Slot.DeleteAll(nextOldPlan);

                //Individua le future manuali da reincludere nel nuovo piano
                var nextManPlan = currPlan.FindAllByPropertyFilter(Filter.Gt(nameof(TaskSchedulazionePiano.DataEsecuzione), dateStart).And(Filter.Eq(nameof(TaskSchedulazionePiano.IsManuale), 1)));
                newPlan.AddRange(nextManPlan);

            }



            //Filtriamo eventuali schedulazioni non a carico di questo nodo master
            return newPlan.Where(p => p.Task.SchedNodoId == nodoId || p.Task.SchedNodoId == 0).ToList();
        }
     

    }
}
