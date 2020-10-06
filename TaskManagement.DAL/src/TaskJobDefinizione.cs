using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("TaskJobDefinizioni")]
    public abstract class TaskJobDefinizione : DataObject<TaskJobDefinizione>
    {
        [PrimaryKey()]
        public abstract int CodTaskJob { get; set; }
        [PrimaryKey()]
        public abstract short Progressivo { get; set; }
        [PrimaryKey()]
        public abstract int CodTask { get; set; }
        public abstract bool AbilitaNotifiche { get; set; }
        public abstract bool Attivo { get; set; }
        public abstract short MinReturnCodeAvvio { get; set; }
    }
}