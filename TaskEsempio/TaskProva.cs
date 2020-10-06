using TaskInterfaceLib;

namespace TaskEsempio
{
    public class TaskProva : TaskBase
    {
        protected override bool CheckUserParams()
        {
            return true;
        }

        protected override int TaskExecute()
        {
            for (int i = 1; i <= 1000; i++)
                this.ReportProgress(i, 1000);

            return 0;
        }
    }
}