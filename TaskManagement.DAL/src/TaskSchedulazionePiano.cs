using Bdo.Attributes;
using Bdo.Objects;
using System;

namespace TaskManagement.DAL
{
    [Table("TaskSchedulazioni_Piano")]
    public abstract class TaskSchedulazionePiano : DataObject<TaskSchedulazionePiano>
    {
        public const string KEY_RUN_DATE = @"KEY_RUN_DATE";

        [PrimaryKey()]
        public abstract long Id { get; set; }

        [SearchKey(KEY_RUN_DATE)]
        public abstract int TaskDefId { get; set; }
        
        [PropertyMap(nameof(TaskDefId))]
        public abstract TaskDefinizione Task { get; }

        [SearchKey(KEY_RUN_DATE)]
        public abstract DateTime DataEsecuzione { get; set; }

        public abstract short StatoEsecuzioneId { get; set; }

        [PropertyMap(nameof(StatoEsecuzioneId))]
        public abstract TaskStatoEsecuzione StatoEsecuzione { get; }

        [AutoInsertTimestamp]
        public abstract DateTime DataInserimento { get; }

        [AutoUpdateTimestamp]
        public abstract DateTime DataAggiornamento { get; set; }

    }
}