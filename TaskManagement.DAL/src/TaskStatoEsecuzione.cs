using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("TaskStatiEsecuzione"), GlobalCache()]
    public abstract class TaskStatoEsecuzione : DataObject<TaskStatoEsecuzione>
    {
        [PrimaryKey()]
        public abstract short Id { get; }
        [MinLength(1)]
        [MaxLength(50)]
        public abstract string Nome { get; }
    }
}