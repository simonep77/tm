using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("TaskTipiFile"), GlobalCache()]
    public abstract class TaskTipoFile : DataObject<TaskTipoFile>
    {
        [PrimaryKey()]
        public abstract short Id { get; }
        [MinLength(1)]
        [MaxLength(100)]
        public abstract string Nome { get; }
    }
}