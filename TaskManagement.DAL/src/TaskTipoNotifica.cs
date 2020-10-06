using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("TaskTipiNotifiche"), GlobalCache()]
    public abstract class TaskTipoNotifica : DataObject<TaskTipoNotifica>
    {
        [PrimaryKey()]
        public abstract short Id { get; }
        [MinLength(1)]
        [MaxLength(50)]
        public abstract string Nome { get; }
    }
}