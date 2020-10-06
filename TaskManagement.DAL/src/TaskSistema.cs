using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("TaskSistemi"), GlobalCache()]
    public abstract class TaskSistema : DataObject<TaskSistema>
    {
        [PrimaryKey()]
        public abstract short Id { get; }
        [MinLength(1)]
        [MaxLength(50)]
        public abstract string Nome { get; }
    }
}