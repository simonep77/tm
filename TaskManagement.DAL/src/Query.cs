using Bdo.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskManagement.DAL
{
    public class Query
    {

        /// <summary>
        /// Calcola hash delle schedulazioni dei reports
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public static string CalculateAutoSchedulesHash(BusinessSlot slot)
        {
            var sql = new StringBuilder(@"SET group_concat_max_len = 1024 * 1024;");
            sql.AppendLine();
            sql.Append($"SELECT IFNULL(SHA1(GROUP_CONCAT(IFNULL(CONCAT(e.{nameof(TaskDefinizione.Id)},'+',e.{nameof(TaskDefinizione.SchedCronString)}), '') ORDER BY e.Id SEPARATOR ';')), '') ");
            sql.AppendLine();
            sql.Append($"FROM {slot.DbPrefixGetTableName<TaskDefinizione>()} e ");
            sql.AppendLine();
            sql.Append($"WHERE e.{nameof(TaskDefinizione.Attivo)}=1 ");

            slot.DB.SQL = sql.ToString();

            return slot.DB.ExecScalar().ToString();

        }


        /// <summary>
        /// Calcola hash delle schedulazioni manuali
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public static string CalculateManualSchedulesHash(BusinessSlot slot)
        {
            var sql = new StringBuilder(@"SET group_concat_max_len = 1024 * 1024;");
            sql.AppendLine();
            sql.Append($"SELECT IFNULL(SHA1(GROUP_CONCAT(IFNULL(CONCAT(e.{nameof(TaskSchedulazionePiano.TaskDefId)},'+',e.{nameof(TaskSchedulazionePiano.DataEsecuzione)}), '') ORDER BY e.Id SEPARATOR ';')), '') ");
            sql.AppendLine();
            sql.Append($"FROM {slot.DbPrefixGetTableName<TaskSchedulazionePiano>()} e ");
            sql.AppendLine();
            sql.Append($"WHERE e.{nameof(TaskSchedulazionePiano.IsManuale)}>0 ");

            slot.DB.SQL = sql.ToString();

            return slot.DB.ExecScalar().ToString();

        }



    }
}
