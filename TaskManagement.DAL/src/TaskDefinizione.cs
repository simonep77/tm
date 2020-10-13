using System;
using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("TaskDefinizioni")]
    public abstract class TaskDefinizione : DataObject<TaskDefinizione>
    {
        public const string KEY_NOME = @"KEY_NOME";

        [PrimaryKey()]
        public abstract int Id { get; set; }
        
        [MinLength(1)]
        [MaxLength(50)]
        [SearchKey(KEY_NOME)]
        public abstract string Nome { get; set; }
        public abstract short SistemaId { get; set; }

        [PropertyMap(nameof(SistemaId))]
        public abstract TaskSistema Sistema { get; }

        public abstract sbyte Attivo { get; set; }
        public abstract short TipoTaskId { get; set; }
        
        [PropertyMap(nameof(TipoTaskId))]
        public abstract TaskTipo TipoTask { get; }
        [MinLength(1)]
        [MaxLength(300)]
        public abstract string AssemblyPath { get; set; }
        [MinLength(1)]
        [MaxLength(100)]
        public abstract string TaskClass { get; set; }
        [MinLength(1)]
        [MaxLength(250)]
        public abstract string LogDir { get; set; }
        [MinLength(1)]
        [MaxLength(250)]
        public abstract string DatiDir { get; set; }
        public abstract bool MostraConsole { get; set; }
        public abstract short TipoNotificaId { get; set; }
        [AcceptNull()]
        [MaxLength(50)]
        public abstract string MailFROM { get; set; }
        [AcceptNull()]
        [MaxLength(50)]
        public abstract string MailTO { get; set; }
        [AcceptNull()]
        [MaxLength(50)]
        public abstract string MailCC { get; set; }
        [AcceptNull()]
        [MaxLength(50)]
        public abstract string MailBCC { get; set; }
        [AcceptNull()]
        [MaxLength(50)]
        public abstract string Riferimento { get; set; }
        [AcceptNull()]
        [MaxLength(500)]
        public abstract string Note { get; set; }
        public abstract short MantieniNumLogDB { get; set; }
        public abstract short MantieniNumLogFS { get; set; }
        public abstract sbyte Eliminato { get; set; }
        public abstract DateTime DataInizio { get; set; }
        [AcceptNull()]
        public abstract DateTime DataFine { get; set; }
    }
}