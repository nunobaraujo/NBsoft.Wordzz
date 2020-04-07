using Microsoft.Extensions.DependencyInjection;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Middleware.Validator;
using NBsoft.Wordzz.Models;
using NBsoft.Wordzz.Repositories;
using NBsoft.Wordzz.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Extensions
{
    public static class ServiceCollectionExtensions
    {
        static IServiceCollection RegisterSettings(this IServiceCollection src, AppSettings settings)
        {
            return src
                .AddSingleton(settings);
        }
        static IServiceCollection RegisterRepositories(this IServiceCollection src, AppSettings settings)
        {   
            return src
                .AddSingleton(x => RepositoryFactory.CreateSessionRepository(
                    RepositoryFactory.RepositoryType.MySql, 
                    settings.Wordzz.Db.SessionConnString, 
                    x.GetRequiredService<ILogger>()));
        }
        static IServiceCollection RegisterServices(this IServiceCollection src)
        {
            return src.AddSingleton<ILicenseService, LicenseService>()
                .AddSingleton<ISessionService, SessionService>()
                .AddSingleton<IValidator>(x => new SessionKeyValidator());
        }
    }
}
