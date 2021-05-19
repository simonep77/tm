using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskmanagement.Scheduler.Common
{
    public static class ExtentionMethods
    {

        public static bool OpIN<T>(this T obj, params T[] args)
            where T: IComparable<T>
        {
            if (args == null || args.Length == 0)
                return false;

            for (int i = 0; i < args.Length; i++)
            {
                if (obj.CompareTo(args[i]) == 0)
                    return true;
            }

            return false;
        }

    }
}
