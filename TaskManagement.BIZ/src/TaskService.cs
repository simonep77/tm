using Bdo.Objects;
using System;
using TaskManagement.Common;
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
        public void RebuildPlanByTaskId(int taskId, int numOfdays, bool isMaster, int nodoId)
        {
            var tdefBiz = this.Slot.BizNewWithLoadByPK<TaskDefinizioneBiz>(taskId);

            tdefBiz.ReBuildSchedulePlan(DateTime.Now.AddDays(numOfdays), isMaster, nodoId);
        }

        /// <summary>
        /// Esegue schedulazione
        /// </summary>
        /// <param name="pianoSchedId"></param>
        public void RunTaskByPianoSchedId(long pianoSchedId)
        {
            var piano = this.Slot.LoadObjByPK<TaskSchedulazionePiano>(pianoSchedId);
            var tdefBiz = piano.Task.ToBizObject<TaskDefinizioneBiz>();
            //Imposta il parametro di piano in esecuzione
            tdefBiz.PianoSchedulazioneId = piano.Id;

            //Scrive avvio schedulazione
            piano.StatoEsecuzioneId = EStatoEsecuzione.PS_InEsecuzione;
            this.Slot.SaveObject(piano);

            //Esegue schedulazione
            tdefBiz.Run();

            //Scrive fine schedulazione
            piano.StatoEsecuzioneId = tdefBiz.UltimaEsecuzione.ReturnCode == 0 ? EStatoEsecuzione.PS_TerminatoConSuccesso : EStatoEsecuzione.PS_TerminatoConErrori;
            this.Slot.SaveObject(piano);
        }




    }
}
