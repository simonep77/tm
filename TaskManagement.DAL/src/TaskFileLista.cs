using Bdo.Objects;
using System.Text;

namespace TaskManagement.DAL
{
    public class TaskFileLista : DataList<TaskFileLista, TaskFile>
    {

        public TaskFileLista CercaFilesPerTaskDefId(int taskDefId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"SELECT tf.Id ");
            sb.Append($"FROM {this.Slot.DbPrefixGetTableName<TaskFile>()} tf ");
            sb.Append($"INNER JOIN {this.Slot.DbPrefixGetTableName<TaskEsecuzione>()} te ON te.Id=tf.TaskEsecuzioneId ");
            sb.Append(@"WHERE te.TaskDefId = @TASKDEFID ");
            sb.Append(@"ORDER BY tf.Id DESC ");

            this.Slot.DB.SQL = sb.ToString();
            this.Slot.DB.AddParameter(@"@TASKDEFID", taskDefId);

            return this.DoSearch();
        }

    }
}