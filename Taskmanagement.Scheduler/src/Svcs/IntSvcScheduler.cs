using Bdo.Objects;
using FluentScheduler;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Taskmanagement.Scheduler.Common;
using TaskManagement.BIZ.src;
using TaskManagement.DAL;

namespace Taskmanagement.Scheduler.Svcs
{
    public class IntSvcScheduler
    {

        public string Schedule_Last_Manual_Hash { get; set; } = string.Empty;
        public string Schedule_Last_Auto_Hash { get; set; } = string.Empty;
        public DateTime Schedule_Last_Refresh { get; set; } = DateTime.MinValue;
        public int Node_ID { get; set; }

        public void Start()
        {
            var role = AppContextTM.SCHEDULE_MASTER_NODE ? "Ruolo: MASTER [Gestione piano schedulazione ed esecuzione dei task senza nodo assegnato]" : "Ruolo: SLAVE [esecuzione dei soli task assegnati esplicitamente]";
            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, @"Schedulatore interno job inizializzazione...");
            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, role);
            //Crea lo scheduler principale
            FluentScheduler.JobManager.Initialize();
            FluentScheduler.JobManager.Start();

            //Aggiunge i log vari
            FluentScheduler.JobManager.JobStart += (j) => { if (!j.Name.StartsWith("SYS_", StringComparison.Ordinal)) AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"Job {j.Name} avviato"); };
            FluentScheduler.JobManager.JobEnd += (j) => { if (!j.Name.StartsWith("SYS_", StringComparison.Ordinal)) AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"Job {j.Name} terminato"); };
            FluentScheduler.JobManager.JobException += (j) => AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Error, $"Job {j.Name} in errore: {j.Exception.Message}");

            //Registra proprio accesso
            this.caricaDatiNodo();

            //Carica schedulazioni
            this.taskUpdatePlan(true);

            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, @"Schedulatore interno job avviato");
        }

        #region GESTIONE NODO


        private void caricaDatiNodo()
        {
            using (var slot = AppContextTM.Service.CreateSlot())
            {

                var nodo = slot.LoadObjOrNewByKEY<TaskNodo>(TaskNodo.KEY_HOST, Environment.MachineName);

                if (nodo.ObjectState == EObjectState.New)
                {
                    nodo.Hostname = Environment.MachineName;

                    try
                    {
                        nodo.FQDN = System.Net.Dns.GetHostEntry("localhost").HostName;
                    }
                    catch (Exception ex)
                    {
                        AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Warning, $"Errore nell'ottenimento dell'FQDN: {ex.Message}");
                        nodo.FQDN = Environment.MachineName;
                    }

                    slot.SaveObject(nodo);

                    AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"Nodo registrato con successo. Id: {nodo.Id}");
                }
                else
                {
                    AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"Avvio nodo gia' registrato con Id: {nodo.Id}");
                }

                //Legge
                this.Node_ID = nodo.Id;
                //Aggiorna
                nodo.RunIsMaster = AppContextTM.SCHEDULE_MASTER_NODE ? (sbyte)1 : (sbyte)0;
                nodo.RunStart = DateTime.Now;
                nodo.RunEnd = DateTime.MinValue;
                nodo.RunPID = Process.GetCurrentProcess().Id.ToString();
                slot.SaveObject(nodo);
            }
        }

        private void terminaNodo()
        {
            using (var slot = AppContextTM.Service.CreateSlot())
            {
                var nodo = slot.LoadObjNullByKEY<TaskNodo>(TaskNodo.KEY_HOST, Environment.MachineName);

                if (nodo == null)
                    return;

                //Aggiorna
                nodo.RunEnd = DateTime.Now;
                slot.SaveObject(nodo);
            }
        }


        #endregion


        public void Stop()
        {
            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, @"Schedulatore interno job in chiusura...");
            //Chiude info nodo
            this.terminaNodo();
            //Stoppa schedulatore
            FluentScheduler.JobManager.StopAndBlock();

            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, @"Schedulatore interno job terminato");
        }


        public void DeleteAllSchedules()
        {
            FluentScheduler.JobManager.RemoveAllJobs();
        }


        private void printSchedules()
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"Elenco prossime schedulazioni: ");


            var trsOrdered = JobManager.AllSchedules.Where(x => !x.Name.StartsWith("SYS_", StringComparison.Ordinal)).OrderBy(t => t.NextRun);

            foreach (var tr in trsOrdered)
            {
                sb.AppendFormat($" > {tr.NextRun:dd/MM/yyyy HH:mm} -  {tr.Name}");
                sb.AppendLine();
            }


            if (AppContextTM.Service.RunMode == CostantiSched.RunMode.Console)
            {
                AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"Schedulazioni caricate: {trsOrdered.Count()}");
                AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, sb.ToString());
            }
            else
                AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"Schedulazioni caricate: {trsOrdered.Count()}");

        }




        public void ReloadSchedules()
        {
            //Mette in pausa tutte le schedulazioni
            JobManager.Stop();
            try
            {
                this.DeleteAllSchedules();

                //Aggiunge check aggiornamento Piano
                JobManager.AddJob(() =>
                {
                    //Avvia esecuzione ed attende fine
                    this.taskUpdatePlan(false);

                }, s => s.WithName($"SYS_CheckAggiornamentoPiano").ToRunEvery(AppContextTM.SCHEDULE_FORCED_UPDATE_CHECK_SECONDS).Seconds());
                //Aggiunge estensione Piano
                JobManager.AddJob(() =>
                {
                    //Avvia esecuzione ed attende fine
                    this.taskUpdatePlan(true);

                }, s => s.WithName($"SYS_AggiornamentoGiornalieroPiano").ToRunEvery(1).Days().At(0, 7));

                var dtPlanStart = DateTime.Now;
                var dtPlanEnd = dtPlanStart.AddDays(AppContextTM.SCHEDULE_EXECUTION_PLAN_DAYS);

                //Rigeneriamo il piano di esecuzione
                using (var slot = AppContextTM.Service.CreateSlot())
                {
                    //per sicurezza vengono caricati tutti i task
                    var tasks = slot.CreateList<TaskDefinizioneLista>()
                        .LoadFullObjects()
                        .SearchAllObjects();


                    //Esegue rebuild piano
                    foreach (var task in tasks)
                    {
                        try
                        {
                            var tBiz = task.ToBizObject<TaskDefinizioneBiz>();
                            //Viene utilizzata la notazione senza secondi NCrontab (Cron.guru)
                            var schedPlan = tBiz.ReBuildSchedulePlan(dtPlanEnd, AppContextTM.SCHEDULE_MASTER_NODE, this.Node_ID);

                            foreach (var piano in schedPlan)
                            {
                                var planId = piano.Id;
                                //Schedula il task utente
                                JobManager.AddJob(() =>
                                {
                                    //Avvia esecuzione ed attende fine
                                    this.taskRunJob(planId);

                                }, s => s.WithName($"{piano.Task.Nome} {piano.Id}").ToRunOnceAt(piano.DataEsecuzione));
                            }
                        }
                        catch (Exception e)
                        {
                            AppContextTM.Service.WriteLog(EventLogEntryType.Error, $"Errore nella schedulazione del task: {task.Nome}");
                            AppContextTM.Service.SendMailError($"Errore nella schedulazione del task: {task.Nome}<br>{e.Message}<br><br>{e.StackTrace}");
                        }

                    }

                }

                this.printSchedules();

            }
            catch (Exception e)
            {
                AppContextTM.Service.WriteLog(EventLogEntryType.Error, $"Errore nell'aggiornamento del piano di schedulazione");
                AppContextTM.Service.SendMailError($"Errore nell'aggiornamento del piano di schedulazione: <br>{e.Message}<br><br>{e.StackTrace}");
            }
            finally
            {
                //Riavvia tutte le schedulazioni
                JobManager.Start();
            }
        }

        private void taskRunJob(long planId)
        {
            AppContextTM.Service.TaskRunByPlanId(planId, false);
        }

        private void taskUpdatePlan(bool force)
        {
            //Preimposta reload con indicazione di forzatura
            var bEseguiReload = force;

            bEseguiReload |= string.IsNullOrWhiteSpace(AppContextTM.Service.InternalScheduler.Schedule_Last_Auto_Hash);

            using (var slot = AppContextTM.Service.CreateSlot())
            {
                var hashAutoSchedules = Query.CalculateAutoSchedulesHash(slot);
                var hashManSchedules = Query.CalculateManualSchedulesHash(slot);

                bEseguiReload |= (hashAutoSchedules != AppContextTM.Service.InternalScheduler.Schedule_Last_Auto_Hash);
                bEseguiReload |= (hashManSchedules != AppContextTM.Service.InternalScheduler.Schedule_Last_Manual_Hash);

                AppContextTM.Service.InternalScheduler.Schedule_Last_Auto_Hash = hashAutoSchedules;
                AppContextTM.Service.InternalScheduler.Schedule_Last_Manual_Hash = hashManSchedules;
                AppContextTM.Service.InternalScheduler.Schedule_Last_Refresh = DateTime.Now;
            }

            if (bEseguiReload)
                this.ReloadSchedules();

        }

    }
}
