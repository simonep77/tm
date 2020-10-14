using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("tasktipitask"), GlobalCache()]
    public abstract class TaskTipo : DataObject<TaskTipo>
    {
        [PrimaryKey()]
        public abstract short Id { get; }
        [MinLength(1)]
        [MaxLength(100)]
        public abstract string Nome { get; }
    }
}