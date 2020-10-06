using System;
using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("TaskFiles")]
    public abstract class TaskFile : DataObject<TaskFile>
    {
        [PrimaryKey()]
        public abstract long CodTaskEsecuzione { get; set; }
        [PrimaryKey()]
        public abstract short CodTipoFile { get; set; }
        [MinLength(1)]
        [MaxLength(150)]
        public abstract string FileName { get; set; }
        [LoadOnAccess()]
        public abstract byte[] FileData { get; set; }
        public abstract DateTime DataInserimento { get; set; }
        public abstract DateTime DataAggiornamento { get; set; }
    }
}