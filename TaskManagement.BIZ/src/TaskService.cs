using Bdo.Objects;
using System;
using TaskManagement.DAL;

namespace TaskManagement.BIZ.src
{
    /// <summary>
    /// Classe per l'accesso alle funzionalita' TM
    /// </summary>
    public class TaskService: DataUtil
    {

        public TaskService(string dbType, string connStr) : this(new BusinessSlot(dbType, connStr))
        {
        }


        public TaskService(BusinessSlot slot): base(slot)
        {
            this.Slot.Conf.ObjectValidationUseMessageList = false;
            this.Slot.DB.AutoCloseConnection = true;
        }


        public void RunTaskByName(string name)
        {
            var tdefBiz = this.Slot.BizNewWithLoadByKEY<TaskDefinizioneBiz>(TaskDefinizione.KEY_NOME, name);

            tdefBiz.Run();
        }


        /// <summary>
        /// dato un task esegue la rigenerazione del piano di schedulazione
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="numOfdays"></param>
        public void RebuildPlanByTaskId(int taskId, int numOfdays)
        {
            var tdefBiz = this.Slot.BizNewWithLoadByPK<TaskDefinizioneBiz>(taskId);

            tdefBiz.ReBuildSchedulePlan(DateTime.Now.AddDays(numOfdays));
        }


        public void RunTaskByPianoSchedId(long pianoSchedId)
        {
            var piano = this.Slot.LoadObjByPK<TaskSchedulazionePiano>(pianoSchedId);
            var tdefBiz = piano.Task.ToBizObject<TaskDefinizioneBiz>();
            //Imposta il parametro di piano in esecuzione
            tdefBiz.PianoSchedulazioneId = piano.Id;

            tdefBiz.Run();
        }




    }
}
