using Bdo.Attributes;
using Bdo.Objects;
using System;

namespace TaskManagement.DAL
{
    [Table("taskdettaglijob")]
    public abstract class TaskDettaglioJob : DataObject<TaskDettaglioJob>
    {
        [PrimaryKey(), AutoIncrement()]
        public abstract int Id { get; }
        public abstract int JobTaskDefId { get; set; }

        [PropertyMap(nameof(JobTaskDefId))]
        public abstract TaskDefinizione JobTask { get; }

        public abstract int SubTaskDefId { get; set; }
        [PropertyMap(nameof(SubTaskDefId))]
        public abstract TaskDefinizione SubTask { get; }
        
        public abstract int Progressivo { get; set; }
        public abstract sbyte Attivo { get; set; }

        public abstract short MinPredReturnCode { get; set; }
        public abstract sbyte AbilitaNotifiche { get; set; }
        public abstract DateTime DataInizio { get; set; }
        public abstract DateTime DataFine { get; set; }

    }
}