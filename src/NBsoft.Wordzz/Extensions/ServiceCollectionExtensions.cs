using Microsoft.Extensions.DependencyInjection;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Models;
using NBsoft.Wordzz.Repositories;
using NBsoft.Wordzz.Services;
using System;

namespace NBsoft.Wordzz.Extensions
{
    internal  static class ServiceCollectionExtensions
    {     
        internal static IServiceCollection RegisterRepositories(this IServiceCollection src, AppSettings settings)
        {            
            var dbType = (RepositoryFactory.RepositoryType)Enum.Parse(typeof(RepositoryFactory.RepositoryType), settings.Wordzz.Db.DbType);

            return src
                .AddSingleton<RepositoryFactory>()
                .AddTransient(x => x.GetRequiredService<RepositoryFactory>().CreateUserRepository())
                .AddTransient(x => x.GetRequiredService<RepositoryFactory>().CreateSessionRepository())
                .AddTransient(x => x.GetRequiredService<RepositoryFactory>().CreateWordRepository());
        }
        internal static IServiceCollection RegisterServices(this IServiceCollection src)
        {
            return src.AddSingleton<ILicenseService, LicenseService>()
                .AddScoped<ISessionService, SessionService>();
        }
    }
}
