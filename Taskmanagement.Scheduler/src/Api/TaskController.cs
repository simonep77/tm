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
using TaskManagement.BIZ.src;
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
            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"Checking API KEY..");

            if (Debugger.IsAttached)
                return true;

            OwinContext owin = (OwinContext)this.Request.Properties["MS_OwinContext"];


            string key = owin.Request.Headers["apikey"];
            
            if (string.IsNullOrWhiteSpace(key))
                key = owin.Request.Query["apikey"];


            if (string.IsNullOrWhiteSpace(key))
            {
                AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Warning, $"API KEY non fornita");
            }
            else
            {
                var bFound = AppContextTM.API_KEYS.Where(k => k == key).Any();

                if (!bFound)
                    AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Warning, $"API KEY {key} nessun match");
            }


            return false;
        }

        //[HttpPost]
        //[HttpGet]
        //[Route(@"api/task/{taskName}/run")]
        //public object TaskRun(string taskName)
        //{
        //    AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"API Call -> run {taskName}");
        //    try
        //    {
        //        if (!this.checkApiKeys())
        //            return this.ResponseMessage(new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "apikey non fornita o non valida" });


        //        var taskDal = this.Slot.LoadObjNullByKEY<TaskDefinizione>(TaskDefinizione.KEY_NOME, taskName);

        //        if (taskDal == null)
        //            return this.NotFound();

        //        var tBiz = taskDal.ToBizObject<TaskDefinizioneBiz>();


        //        var tSched = tBiz.CreaSchedulazione(DateTime.Now, true, true);

        //        AppContextTM.Service.TaskRunByPlanId(tSched.Id, false);

        //        this.Slot.RefreshObject(tSched, true);

        //        return new
        //        {
        //            RunId = tSched.Id,
        //            StateId = tSched.StatoEsecuzioneId,
        //            StateDesc = tSched.StatoEsecuzione.Nome,
        //            DataEsecuzione = tSched.DataEsecuzione.ToString("yyyyMMddHHmmss"),
        //            TaskName = tSched.Task.Nome,
        //            TaskId = tSched.TaskDefId
        //        };
        //    }
        //    catch (Exception e)
        //    {
        //        AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Error, $"API Call -> error: {e.Message}");
        //        return this.InternalServerError(e);
        //    }

        //}


        [HttpPost]
        [HttpGet]
        [Route(@"api/task/{taskName}/run/{when?}")]
        public object TaskSchedule(string taskName, string when = null)
        {
            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"API Call -> run {taskName} {when}");
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

                return new
                {
                    RunId = tSched.Id,
                    StateId = tSched.StatoEsecuzioneId,
                    StateDesc = tSched.StatoEsecuzione.Nome,
                    DataEsecuzione = tSched.DataEsecuzione.ToString("yyyyMMddHHmmss"),
                    TaskName = tSched.Task.Nome,
                    TaskId = tSched.TaskDefId
                };
            }
            catch (Exception e)
            {
                AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Error, $"API Call -> error: {e.Message}");
                AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Error, $"{e.StackTrace}");
                return this.InternalServerError(e);
            }
            
        }


        [HttpGet]
        [Route(@"api/task/{planId}/status")]
        public object TaskStatus(long planId)
        {
            AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Information, $"API Call -> status {planId}");
            try
            {
                if (!this.checkApiKeys())
                    return this.ResponseMessage(new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "apikey non fornita o non valida" });

                var tSched = this.Slot.LoadObjNullByPK<TaskSchedulazionePiano>(planId);

                if (tSched == null)
                    this.NotFound();

                return new
                {
                    RunId = tSched.Id,
                    StateId = tSched.StatoEsecuzioneId,
                    StateDesc = tSched.StatoEsecuzione.Nome,
                    DataEsecuzione = tSched.DataEsecuzione.ToString("yyyyMMddHHmmss"),
                    TaskName = tSched.Task.Nome,
                    TaskId = tSched.TaskDefId
                };
            }
            catch (Exception e)
            {
                AppContextTM.Service.WriteLog(System.Diagnostics.EventLogEntryType.Error, $"API Call -> error: {e.Message}");
                return this.InternalServerError(e);
            }

        }


    }
}
