using Bdo.Attributes;
using Bdo.Objects;
using System;

namespace TaskManagement.DAL
{
    [Table("TaskSchedulazioni_Piano")]
    public abstract class TaskSchedulazionePiano : DataObject<TaskSchedulazionePiano>
    {
        [PrimaryKey()]
        public abstract long Id { get; set; }
        public abstract int TaskDefId { get; set; }
        
        [PropertyMap(nameof(TaskDefId))]
        public abstract TaskDefinizione Task { get; }
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