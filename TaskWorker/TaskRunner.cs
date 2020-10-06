using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskInterfaceLib
{
    class TaskRunner
    {


        public void Crea()
        {
           
            var appDom = AppDomain.CreateDomain(@"MyTaskDomain");
            try
            {
                //Crea istanza nell'altro dominio
                var task = (ITaskTM)appDom.CreateInstanceFromAndUnwrap("", "");

                task.OnTaskStart += CreaEsecuzioneTask;
                task.OnTaskEnd += AggiornaFineEsecuzioneTask;
                task.OnTaskError += AggiornaErroreEsecuzionetask;
                task.OnReportSegnalazione += SalvaSegnalazioneTask;

                task.Execute();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                AppDomain.Unload(appDom);
            }

        }

        private void SalvaSegnalazioneTask(ITaskTM task, ETipoSegnalazione cond, string itemKey, string itemType, string text)
        {
            throw new NotImplementedException();
        }

        private void AggiornaErroreEsecuzionetask(ITaskTM task, Exception e)
        {
            throw new NotImplementedException();
        }

        private void AggiornaFineEsecuzioneTask(ITaskTM task)
        {
            throw new NotImplementedException();
        }

        private void CreaEsecuzioneTask(ITaskTM task)
        {
            throw new NotImplementedException();
        }
    }
}
