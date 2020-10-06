using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("TaskParametri")]
    public abstract class TaskParametro : DataObject<TaskParametro>
    {
        [PrimaryKey()]
        public abstract int TaskDefId { get; set; }
        [PrimaryKey()]
        [MinLength(1)]
        [MaxLength(50)]
        public abstract string Chiave { get; set; }
        [MinLength(1)]
        [MaxLength(500)]
        public abstract string Valore { get; set; }
        public abstract bool Visibile { get; set; }
    }
}