using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("tasktipinotifiche"), GlobalCache()]
    public abstract class TaskTipoNotifica : DataObject<TaskTipoNotifica>
    {
        [PrimaryKey()]
        public abstract short Id { get; }
        [MinLength(1)]
        [MaxLength(100)]
        public abstract string Nome { get; }
    }
}