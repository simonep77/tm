using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("TaskTipiTask"), GlobalCache()]
    public abstract class TaskTipo : DataObject<TaskTipo>
    {
        [PrimaryKey()]
        public abstract short Id { get; }
        [MinLength(1)]
        [MaxLength(50)]
        public abstract string Nome { get; }
    }
}