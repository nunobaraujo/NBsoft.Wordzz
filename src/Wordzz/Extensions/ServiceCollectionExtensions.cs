using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Cache;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Contracts.Settings;
using NBsoft.Wordzz.Core.Cache;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.DictionaryApi;
using NBsoft.Wordzz.Entities;
using NBsoft.Wordzz.Models;
using NBsoft.Wordzz.Repositories;
using NBsoft.Wordzz.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Extensions
{
    internal  static class ServiceCollectionExtensions
    {     
        internal static IServiceCollection RegisterFactories(this IServiceCollection src, AppSettings settings)
        {
            return src
                .AddTransient<RepositoryFactory>()
                .AddTransient<DictionaryApiFactory>();
        }

        internal static IServiceCollection RegisterRepositories(this IServiceCollection src, AppSettings settings)
        {            
            var dbType = (RepositoryFactory.RepositoryType)Enum.Parse(typeof(RepositoryFactory.RepositoryType), settings.Wordzz.Db.DbType);

            return src
                .AddTransient(x => x.GetRequiredService<RepositoryFactory>().CreateUserRepository())
                .AddTransient(x => x.GetRequiredService<RepositoryFactory>().CreateSessionRepository())
                .AddTransient(x => x.GetRequiredService<RepositoryFactory>().CreateGameRepository())
                .AddTransient(x => x.GetRequiredService<RepositoryFactory>().CreateWordRepository())
                .AddTransient(x => x.GetRequiredService<RepositoryFactory>().CreateBoardRepository())
                .AddTransient(x => x.GetRequiredService<RepositoryFactory>().CreateStatsRepository());
        }
        internal static IServiceCollection RegisterCache(this IServiceCollection src)
        {
            return src.AddSingleton<IGameCache, GameCache>();
        }
        internal static IServiceCollection RegisterServices(this IServiceCollection src, AppSettings settings)
        {
            if (settings.Wordzz.UseLexiconCache)
                src.AddSingleton<ILexiconService, CacheLexiconService>();
            else
                src.AddSingleton<ILexiconService, LexiconService>();

            return src.AddSingleton<ISessionService, SessionService>()
                .AddSingleton<IGameService, GameService>()
                .AddSingleton<IGameQueueService, GameQueueService>()
                .AddSingleton<IStatService, StatService>();

        }

        internal static async Task ValidateBoard(this IApplicationBuilder app, ILogger log)
        {
            var serviceProvider = app.ApplicationServices;
            var boardRepository = serviceProvider.GetService<IBoardRepository>();

            // Check if standard board exists
            var boards = await boardRepository.List();
            var standardBoard = boards.SingleOrDefault(b => b.Name == "Standard");
            if (standardBoard == null)
            {
                var newBoard = Helpers.BoardHelper.GenerateBoard(15, "Standard");
                var created = await boardRepository.Add(newBoard);
                await log.InfoAsync("Standard board Added.");
            }
        }
        internal static async Task ValidateAdminUser(this IApplicationBuilder app, ILogger log, WordzzSettings wsettings)
        {
            var serviceProvider = app.ApplicationServices;
            var userRepository = serviceProvider.GetService<IUserRepository>();
            var admin = await userRepository.Get("sa");
            if (admin != null)
                return;

            var user = new User
            {
                UserName = "sa",
                CreationDate = DateTime.UtcNow,
                Deleted = false
            };
            var withPassword = user.SetPassword("#Na123@10", wsettings.EncryptionKey);
            var added = await userRepository.Add(withPassword, "geral@nbsoft.pt");

            var details = new UserDetails
            {
                UserName = added.UserName,
                Address = "Famalicão",
                City = "Vila Nova de Famalição",
                Country = "Portugal",
                Email = "geral@nbsoft.pt",
                FirstName = "Server",
                LastName = "Admin",
                PostalCode = "4760"
            };
            var addedDetails = await userRepository.UpdateDetails(details);

            var settings = new UserSettings
            {
                UserName = addedDetails.UserName,
                MainSettings = new MainSettings { UserId = addedDetails.UserName, DefaultBoard = 1, Language = "en-us" }.ToJson(),
                WindowsSettings = new WindowsSettings { UserId = addedDetails.UserName }.ToJson(),
                AndroidSettings = new AndroidSettings { UserId = addedDetails.UserName }.ToJson(),
                IOSSettings = new IOSSettings { UserId = addedDetails.UserName }.ToJson()
            };
            await userRepository.UpdateSettings(settings);
            await log.WarningAsync("Added server admin user");

        }
    }
}
