using Bdo.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpPost]
        [HttpGet]
        [Route(@"api/task/{taskName}/run")]
        public object TaskRun(string taskName)
        {

            var taskDal = this.Slot.LoadObjNullByKEY<TaskDefinizione>(TaskDefinizione.KEY_NOME, taskName);

            if (taskDal == null)
            {
                return this.NotFound();
            }

            var tBiz = taskDal.ToBizObject<TaskDefinizioneBiz>();


            var tSched = tBiz.CreaSchedulazione(DateTime.Now, true, true);

            AppContextTM.Service.TaskRunByPlanId(tSched.Id, false);

            this.Slot.RefreshObject(tSched, true);

            return new { 
                RunId = tSched.Id,
                StateId = tSched.StatoEsecuzioneId,
                StateDesc = tSched.StatoEsecuzione.Nome,
                DataEsecuzione = tSched.DataEsecuzione.ToString("yyyyMMddHHmmss"),
                TaskName = tSched.Task.Nome,
                TaskId = tSched.TaskDefId
            };
        }

        [HttpGet]
        [Route(@"api/task/{planId}/status")]
        public object TaskStatus(long planId)
        {
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


    }
}
