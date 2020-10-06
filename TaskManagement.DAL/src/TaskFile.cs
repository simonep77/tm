using System;
using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("TaskFiles")]
    public abstract class TaskFile : DataObject<TaskFile>
    {
        public const string KEY_EXEC = @"KEY_EXEC";
        [PrimaryKey, AutoIncrement]
        public abstract long Id { get; }
        [SearchKey(KEY_EXEC)]
        public abstract long TaskEsecuzioneId { get; set; }
     
        public abstract short TipoFileId { get; set; }
        [MinLength(1), MaxLength(150)]
        public abstract string FileName { get; set; }
        
        [LoadOnAccess()]
        public abstract byte[] FileData { get; set; }

        [AutoInsertTimestamp]
        public abstract DateTime DataInserimento { get; }

        [AutoUpdateTimestamp]
        public abstract DateTime DataAggiornamento { get; }
    }
}