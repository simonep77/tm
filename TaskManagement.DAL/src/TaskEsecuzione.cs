using System;
using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("taskesecuzioni")]
    public abstract class TaskEsecuzione : DataObject<TaskEsecuzione>
    {
        [PrimaryKey()]
        [AutoIncrement()]
        public abstract long Id { get; }
        public abstract int TaskDefId { get; set; }

        [PropertyMap(nameof(TaskDefId))]
        public abstract TaskDefinizione Task { get; }

        [AcceptNull]
        public abstract long SchedPianoId { get; set; }
        [AcceptNull]
        public abstract long JobEsecuzioneId { get; set; }


        public abstract short StatoEsecuzioneId { get; set; }
        
        [MinLength(1)]
        [MaxLength(100)]
        public abstract string Host { get; set; }
        public abstract string Pid { get; set; }

        public abstract int ReturnCode { get; set; }
        
        [AcceptNull()]
        [MaxLength(65000)]
        public abstract string ReturnMessage { get; set; }
        
        public abstract int NotificaCode { get; set; }
        
        [AcceptNull()]
        [MaxLength(65000)]
        public abstract string NotificaMessage { get; set; }


        [AcceptNull()]
        public abstract DateTime DataTermine { get; set; }

        [AutoInsertTimestamp]
        public abstract DateTime DataInserimento { get; }

        [AutoUpdateTimestamp]
        public abstract DateTime DataAggiornamento { get; }
    }
}