using Bdo.Attributes;
using Bdo.Objects;
using System;

namespace TaskManagement.DAL
{
    [Table("taskschedulazioni_piano")]
    public abstract class TaskSchedulazionePiano : DataObject<TaskSchedulazionePiano>
    {
        public const string KEY_RUN_DATE = @"KEY_RUN_DATE";

        [PrimaryKey, AutoIncrement]
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

        public abstract sbyte IsManuale { get; set; }

        /// <summary>
        /// Se valorizzato puo' contenere un override dei parametri letti da DB. JSON: {"Parametri":[{"Chiave": "chiave1", "Valore": "valore1", "ValoreOpzionale"=""}]}
        /// </summary>
        [AcceptNull]
        public abstract string JsonParametriOverride { get; set; }


        [AutoInsertTimestamp]
        public abstract DateTime DataInserimento { get; }

        [AutoUpdateTimestamp]
        public abstract DateTime DataAggiornamento { get; set; }

    }
}