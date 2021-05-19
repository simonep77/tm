using Bdo.Objects;
using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Taskmanagement.Scheduler.Api;
using Taskmanagement.Scheduler.Common;
using TaskManagement.BIZ.src;
using TaskManagement.Common;
using TaskManagement.DAL;

namespace Taskmanagement.Scheduler.src.Api
{
    public class TaskController: ApiController
    {

        protected BusinessSlot Slot { get; }

        public TaskController()
        {
            this.Slot = AppContextTM.Service.CreateSlot();
        }

        private bool checkApiKeys()
        {
            AppContextTM.Service.WriteLogConsole(System.Diagnostics.EventLogEntryType.Information, $"Checking apikey..");

            if (Debugger.IsAttached)
            {
                AppContextTM.Service.WriteLogConsole(System.Diagnostics.EventLogEntryType.Information, $"Richiesta locale, skip check");
                return true;
            }

            OwinContext owin = (OwinContext)this.Request.Properties["MS_OwinContext"];


            string key = owin.Request.Headers["apikey"];
            
            if (string.IsNullOrWhiteSpace(key))
                key = owin.Request.Query["apikey"];


            if (string.IsNullOrWhiteSpace(key))
            {
                AppContextTM.Service.WriteLogConsole(System.Diagnostics.EventLogEntryType.Warning, $"API KEY non fornita");
            }
            else
            {
                var bFound = AppContextTM.API_KEYS.Where(k => k == key).Any();

                if (bFound)
                {
                    AppContextTM.Service.WriteLogConsole(System.Diagnostics.EventLogEntryType.Information, $"Richiesta locale, skip check");
                    return true;
                }
                else
                    AppContextTM.Service.WriteLogConsole(System.Diagnostics.EventLogEntryType.Warning, $"API KEY {key} nessun match");
            }

            return false;
        }


        [HttpPost]
        [HttpGet]
        [Route(@"api/task/{taskName}/run/{when?}")]
        public object TaskSchedule(string taskName, string when = null)
        {
            AppContextTM.Service.WriteLogConsole(System.Diagnostics.EventLogEntryType.Information, $"Begin API Call -> run {taskName} {when}");
            try
            {
                if (!this.checkApiKeys())
                    return this.ResponseMessage(new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "apikey non fornita o non valida" });

                var taskDal = this.Slot.LoadObjNullByKEY<TaskDefinizione>(TaskDefinizione.KEY_NOME, taskName);

                if (taskDal == null)
                    return this.NotFound();


                OwinContext owin = (OwinContext)this.Request.Properties["MS_OwinContext"];

                string postParamsJson = null;

                if (owin.Request.Method.ToLower() == "post")
                {
                    IFormCollection formData = owin.Request.ReadFormAsync().Result;

                    if (formData.Any())
                    {
                        dynamic dyn = new ExpandoObject();

                        foreach (var item in formData)
                        {
                            ((IDictionary<string, object>)dyn)[item.Key] = item.Value.FirstOrDefault();
                        }

                        postParamsJson = JsonConvert.SerializeObject(dyn);
                    }
                }

                var tBiz = taskDal.ToBizObject<TaskDefinizioneBiz>();

                TaskSchedulazionePiano tSched = null;

                if (string.IsNullOrWhiteSpace(when))
                {
                    tSched = tBiz.CreaSchedulazione(DateTime.Now, true, true, postParamsJson);
                    AppContextTM.Service.TaskRunByPlanId(tSched.Id, false);
                    this.Slot.RefreshObject(tSched, true);
                }
                else
                {
                    DateTime dtWhen;
                    try
                    {
                        dtWhen = DateTime.ParseExact(when, "yyyyMMddHHmm", CultureInfo.CurrentCulture);
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException($"Se fornita la data di schedulazione questa deve essere nel formato yyyyMMddHHmm");
                    }

                    if (dtWhen <= DateTime.Now)
                        throw new ArgumentException($"La data di schedulazione deve essere maggiore dell'ora corrente. Fornita {when}, corrente {DateTime.Now:yyyyMMddHHmm}. Se si vuole eseguire immediatamente rimuovere il parametro data.");

                    tSched = tBiz.CreaSchedulazione(dtWhen, true, false, postParamsJson);
                    AppContextTM.Service.InternalScheduler.ReloadSchedules();
                }

                return new TaskScheduleDTO
                {
                    RunId = tSched.Id,
                    StatoId = tSched.StatoEsecuzioneId,
                    StatoDesc = tSched.StatoEsecuzione.Nome,
                    DataEsecuzione = tSched.DataEsecuzione.ToString("yyyyMMddHHmmss"),
                    TaskNome = tSched.Task.Nome,
                };
            }
            catch (Exception e)
            {
                AppContextTM.Service.WriteLogConsole(System.Diagnostics.EventLogEntryType.Error, $"    -> error: {e.Message}");
                AppContextTM.Service.WriteLogConsole(System.Diagnostics.EventLogEntryType.Error, $"{e.StackTrace}");
                return this.InternalServerError(e);
            }
            finally
            {
                AppContextTM.Service.WriteLogConsole(System.Diagnostics.EventLogEntryType.Information, $"End API Call -> run {taskName} {when}");
            }

        }


        [HttpGet]
        [Route(@"api/task/{planId}/status")]
        public object TaskStatus(long planId)
        {
            AppContextTM.Service.WriteLogConsole(System.Diagnostics.EventLogEntryType.Information, $"Begin API Call -> status {planId}");
            try
            {
                if (!this.checkApiKeys())
                    return this.ResponseMessage(new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "apikey non fornita o non valida" });

                var tSched = this.Slot.LoadObjNullByPK<TaskSchedulazionePiano>(planId);

                if (tSched == null)
                    this.NotFound();

                TaskEsecuzione exec = null;

                if (tSched.StatoEsecuzioneId.OpIN(EStatoEsecuzione.PS_InEsecuzione, 
                    EStatoEsecuzione.PS_TerminatoConSuccesso, 
                    EStatoEsecuzione.PS_TerminatoConErrori))
                {
                    exec = Slot.LoadObjNullByFILTER<TaskEsecuzione>(Filter.Eq(nameof(TaskEsecuzione.SchedPianoId), tSched.Id));
                }


                return new TaskScheduleRunInfoDTO
                {
                    RunId = tSched.Id,
                    StatoId = tSched.StatoEsecuzioneId,
                    StatoDesc = tSched.StatoEsecuzione.Nome,
                    DataEsecuzione = tSched.DataEsecuzione.ToString("yyyyMMddHHmm"),
                    TaskNome = tSched.Task.Nome,
                    DataAvvio = exec?.DataInserimento.ToString("yyyyMMddHHmmss") ?? string.Empty,
                    DataTermine = exec?.DataTermine.ToString("yyyyMMddHHmmss") ?? string.Empty,
                    ReturnCode = exec?.ReturnCode ?? 0,
                    ReturnMessage = exec?.ReturnMessage ?? string.Empty,
                };
            }
            catch (Exception e)
            {
                AppContextTM.Service.WriteLogConsole(System.Diagnostics.EventLogEntryType.Error, $"    -> error: {e.Message}");
                AppContextTM.Service.WriteLogConsole(System.Diagnostics.EventLogEntryType.Error, $"{e.StackTrace}");
                return this.InternalServerError(e);
            }
            finally
            { 
                AppContextTM.Service.WriteLogConsole(System.Diagnostics.EventLogEntryType.Information, $"End API Call -> status {planId}");
            }

        }


    }
}
