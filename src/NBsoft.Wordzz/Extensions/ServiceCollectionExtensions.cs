using Microsoft.Extensions.DependencyInjection;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Middleware.Validator;
using NBsoft.Wordzz.Models;
using NBsoft.Wordzz.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Extensions
{
    public static class ServiceCollectionExtensions
    {
        static IServiceCollection RegisterRepositories(this IServiceCollection src)
        {
            return src;
        }
        static IServiceCollection RegisterServices(this IServiceCollection src, AppSettings settings)
        {
            src.AddSingleton<ILicenseService>(x => new LicenseService(settings.Wordzz, x.GetRequiredService<ILogger>()));

            return src
                .AddSingleton<IValidator>(x => new SessionKeyValidator());
        }
    }
}
