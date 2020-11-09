using Bdo.Attributes;
using Bdo.Objects;
using System;

namespace TaskManagement.DAL
{
    [Table("tasknodi")]
    public abstract class TaskNodo : DataObject<TaskNodo>
    {
        public const string KEY_HOST = @"KEY_HOST";
        [PrimaryKey, AutoIncrement]
        public abstract int Id { get;}

        [MaxLength(50), SearchKey(KEY_HOST)]
        public abstract string Hostname { get; set; }

        [MaxLength(100)]
        public abstract string FQDN { get; set; }

        [AcceptNull]
        public abstract string Note { get; set; }

        [AcceptNull, MaxLength(10)]
        public abstract string RunPID { get; set; }

        [AcceptNull]
        public abstract DateTime RunStart { get; set; }
        
        public abstract sbyte RunIsMaster { get; set; }
        
        [AcceptNull]
        public abstract DateTime RunEnd { get; set; }

        [AutoUpdateTimestamp]
        public abstract DateTime DataAggiornamento { get; set; }

    }
}