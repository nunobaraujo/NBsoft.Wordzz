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
                .AddTransient(x => x.GetRequiredService<RepositoryFactory>().CreateGameRepository())
                .AddTransient(x => x.GetRequiredService<RepositoryFactory>().CreateWordRepository())
                .AddTransient(x => x.GetRequiredService<RepositoryFactory>().CreateBoardRepository())
                .AddTransient(x => x.GetRequiredService<RepositoryFactory>().CreateStatsRepository());
        }
        internal static IServiceCollection RegisterServices(this IServiceCollection src)
        {
            return src.AddSingleton<ILicenseService, LicenseService>()
                .AddScoped<ISessionService, SessionService>()
                .AddSingleton<ILexiconService, LexiconService>()
                .AddSingleton<IGameQueueService, GameQueueService>()
                .AddSingleton<IGameService, GameService>()
                .AddSingleton<IStatService, StatService>();

        }
    }
}
