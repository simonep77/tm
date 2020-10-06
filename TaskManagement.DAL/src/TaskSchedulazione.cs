using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("TaskSchedulazioni")]
    public abstract class TaskSchedulazione : DataObject<TaskSchedulazione>
    {
        [PrimaryKey()]
        public abstract int Id { get; set; }
        public abstract int TaskDefId { get; set; }
        [PropertyMap(nameof(TaskDefId))]
        public abstract TaskDefinizione Task { get; }
        public abstract sbyte Attivo { get; set; }
        [MinLength(1)]
        [MaxLength(40)]
        public abstract string CronString { get; set; }
        [AcceptNull()]
        [MaxLength(50)]
        public abstract string Host { get; set; }
    }
}