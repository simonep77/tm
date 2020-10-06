using Bdo.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskInterfaceLib;
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
            this.Slot.DB.AutoCloseConnection = true;
        }


        public void RunTaskByName(string name)
        {
            var tdefBiz = this.Slot.BizNewWithLoadByKEY<TaskDefinizioneBiz>(TaskDefinizione.KEY_NOME, name);

            tdefBiz.Run();
        }


      

    }
}
