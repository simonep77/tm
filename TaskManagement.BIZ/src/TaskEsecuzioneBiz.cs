using Bdo.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManagement.DAL;

namespace TaskManagement.BIZ.src
{
    public class TaskEsecuzioneBiz: BusinessObject<TaskEsecuzione>
    {
        public Lazy<TaskFileLista> Files { get; }

        public TaskEsecuzioneBiz(TaskEsecuzione obj):base(obj)
        {
            this.Files = new Lazy<TaskFileLista>(() => this.Slot.CreateList<TaskFileLista>().SearchByColumn(Filter.Eq(nameof(TaskFile.TaskEsecuzioneId), this.DataObj.Id)));
        }
    }
}
