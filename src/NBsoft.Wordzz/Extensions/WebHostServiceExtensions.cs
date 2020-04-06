using Microsoft.Extensions.Hosting;
using System.ServiceProcess;

namespace NBsoft.Wordzz.Extensions
{
    static class WebHostServiceExtensions
    {
        public static void RunAsCustomService(this IHost host)
        {
            //var webHostService = new CustomWebHostService(host);
            //ServiceBase.Run(webHostService);
        }
    }
}
