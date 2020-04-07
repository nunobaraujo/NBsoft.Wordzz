using MySql.Data.MySqlClient;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core.Repositories;
using System.Data.SqlClient;
using System;
using System.Linq;
using NBsoft.Wordzz.Models;
using Microsoft.Extensions.Options;

namespace NBsoft.Wordzz.Repositories
{
    internal class RepositoryFactory
    {
        public enum RepositoryType 
        {
            MsSqlServer = 0,
            MySql = 1
        }
        
        private readonly WordzzSettings _settings;
        private readonly ILogger _logger;
        private readonly RepositoryType _repositoryType;

        public RepositoryFactory(IOptions<WordzzSettings> settings, ILogger logger)
        {
            _settings = settings.Value;
            _logger = logger;

            _repositoryType = (RepositoryType)Enum.Parse(typeof(RepositoryType), _settings.Db.DbType);
        }

        public ISessionRepository CreateSessionRepository()
        {

            switch (_repositoryType)
            {
                case RepositoryType.MsSqlServer:
                    return new SessionRepository(_logger, () => new SqlConnection(_settings.Db.SessionConnString), GetUpdateFields);
                case RepositoryType.MySql:
                    return new SessionRepository(_logger, () => new MySqlConnection(_settings.Db.SessionConnString), GetUpdateFields);
                default:
                    throw new ArgumentOutOfRangeException(nameof(_repositoryType));

            }
            
        }
        public IUserRepository CreateUserRepository()
        {
            switch (_repositoryType)
            {
                case RepositoryType.MsSqlServer:
                    return new UserRepository(_logger, () => new SqlConnection(_settings.Db.MainConnString), GetUpdateFields);
                case RepositoryType.MySql:
                    return new UserRepository(_logger, () => new MySqlConnection(_settings.Db.MainConnString), GetUpdateFields);
                default:
                    throw new ArgumentOutOfRangeException(nameof(_repositoryType));

            }

        }

        private static string GetUpdateFields(Type t)
        {
            return string.Join(",", t.GetProperties().Select(x => x.Name + "=@" + x.Name));            
        }

    }
}
