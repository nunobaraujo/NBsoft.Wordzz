using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using NBsoft.Logs.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NBsoft.Wordzz
{
    public class Program
    {
        public static string EnvInfo => Environment.GetEnvironmentVariable("ENV_INFO");
        public static ILogger Log { get; set; }

        public static void Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));
            var builder = CreateHostBuilder(args.Where(arg => arg != "--console").ToArray());

            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                builder.UseContentRoot(pathToContentRoot);
            }
            else
            {
                string appPath = Directory.GetCurrentDirectory();
                builder.UseContentRoot(appPath);
            }

            var host = builder.Build();
            Console.WriteLine($"{PlatformServices.Default.Application.ApplicationName} version {PlatformServices.Default.Application.ApplicationVersion}");
            //if (isService)
            //    host.RunAsCustomService();
            //else
                host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {    
                    webBuilder
                    .UseKestrel()
                    .UseUrls("http://*:5005")
                    .UseStartup<Startup>();
                });
    }
}
