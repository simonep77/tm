using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taskmanagement.Scheduler.Common;

namespace Taskmanagement.Scheduler
{
    class Program
    {

        static int Main(string[] args)
        {
            AppContextTM.Service.RunByMode(searchRunMode(args));

            return 0;
        }


        static int searchRunMode(string[] args)
        {
            foreach (var arg in args)
            {
                switch (arg.ToLower())
                {
                    case @"/console":
                        return CostantiSched.RunMode.Console;
                    case @"/install":
                        return CostantiSched.RunMode.Install;
                    case @"/uninstall":
                        return CostantiSched.RunMode.Uninstall;
                }
            }

            return CostantiSched.RunMode.Service;

        }

    }
}
