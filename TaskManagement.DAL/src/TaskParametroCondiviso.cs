using Bdo.Attributes;
using Bdo.Objects;

namespace TaskManagement.DAL
{
    [Table("taskparametricondivisi")]
    public abstract class TaskParametroCondiviso : DataObject<TaskParametro>
    {
        public const string KEY_CHIAVE = @"KEY_CHIAVE";
        [PrimaryKey()]
        public abstract int Id { get; set; }
        
        [SearchKey(KEY_CHIAVE)]
        [MinLength(1)]
        [MaxLength(50)]
        public abstract string Chiave { get; set; }


        [MinLength(1)]
        public abstract string Valore { get; set; }
        
        [AcceptNull]
        public abstract string ValoreOpzionale { get; set; }

    }
}