using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("taskparametri")]
    public abstract class TaskParametro : DataObject<TaskParametro>
    {
        [PrimaryKey()]
        public abstract int TaskDefId { get; set; }
        [PrimaryKey()]
        [MinLength(1)]
        [MaxLength(50)]
        public abstract string Chiave { get; set; }

        /// <summary>
        /// Se 1 indica che il parametro fa riferimento ad una voce condivisa. I campi valore saranno pertanto ignorati
        /// </summary>
        public abstract sbyte IsCondiviso { get; set; }


        [MinLength(1)]
        public abstract string Valore { get; set; }

        [AcceptNull]
        public abstract string ValoreOpzionale { get; set; }
        public abstract bool Visibile { get; set; }
    }
}