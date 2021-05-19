using System.Web.Http;
using WebActivatorEx;
using Taskmanagement.Scheduler;
using Swashbuckle.Application;
using Taskmanagement.Scheduler.Api;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Taskmanagement.Scheduler.Api
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "TM.Sched.Api");
            })
            .EnableSwaggerUi(c =>
            {
            });
        }
    }
}
