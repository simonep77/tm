using System;
using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("TaskEsecuzioni")]
    public abstract class TaskEsecuzione : DataObject<TaskEsecuzione>
    {
        [PrimaryKey()]
        [AutoIncrement()]
        public abstract long Id { get; }
        public abstract int CodTask { get; set; }

        [PropertyMap(nameof(CodTask))]
        public abstract TaskDefinizione Task { get; }
        [MinLength(1)]
        [MaxLength(32)]
        public abstract string TaskExecId { get; set; }
        public abstract short CodStatoEsecuzione { get; set; }
        [MinLength(1)]
        [MaxLength(50)]
        public abstract string Host { get; set; }
        public abstract int Pid { get; set; }
        public abstract int ReturnCode { get; set; }
        [AcceptNull()]
        [MaxLength(500)]
        public abstract string ReturnMessage { get; set; }
        public abstract int NotificaCode { get; set; }
        [AcceptNull()]
        [MaxLength(500)]
        public abstract string NotificaMessage { get; set; }
        public abstract DateTime DataAvvio { get; set; }
        [AcceptNull()]
        public abstract DateTime DataTermine { get; set; }
        [AcceptNull()]
        public abstract long ParentId { get; set; }
        [AcceptNull()]
        public abstract int SchedId { get; set; }
        public abstract DateTime DataInserimento { get; set; }
        public abstract DateTime DataAggiornamento { get; set; }
    }
}