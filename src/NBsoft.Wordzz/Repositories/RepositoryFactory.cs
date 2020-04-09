using MySql.Data.MySqlClient;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core.Repositories;
using System.Data.SqlClient;
using System;
using System.Linq;
using NBsoft.Wordzz.Models;
using Microsoft.Extensions.Options;
using System.Data;

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
            Func<IDbConnection> conn = _repositoryType switch
            {
                RepositoryType.MsSqlServer => () => new SqlConnection(_settings.Db.SessionConnString),
                RepositoryType.MySql => () => new MySqlConnection(_settings.Db.SessionConnString),
                _ => throw new ArgumentOutOfRangeException(nameof(_repositoryType)),
            };
            return new SessionRepository(_logger, conn, GetUpdateFields);
        }
        public IUserRepository CreateUserRepository()
        {
            Func<IDbConnection> conn = _repositoryType switch
            {
                RepositoryType.MsSqlServer => () => new SqlConnection(_settings.Db.MainConnString),
                RepositoryType.MySql => () => new MySqlConnection(_settings.Db.MainConnString),
                _ => throw new ArgumentOutOfRangeException(nameof(_repositoryType)),
            };
            return new UserRepository(_logger, conn, GetUpdateFields, GetInsertFields, _settings.EncryptionKey);
        }

        public IWordRepository CreateWordRepository()
        {
            Func<string> getLastId;
            Func<IDbConnection> conn;

            switch (_repositoryType)
            {
                case RepositoryType.MsSqlServer:
                    conn = () => new SqlConnection(_settings.Db.MainConnString);
                    getLastId = GetSqlLastInsertedId;
                    break;
                case RepositoryType.MySql:
                    conn = () => new MySqlConnection(_settings.Db.MainConnString);
                    getLastId = GetMySqlLastInsertedId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_repositoryType));
            }
            return new WordRepository(_logger, conn, GetUpdateFields, GetInsertFields, getLastId);
        }

        private static string GetUpdateFields(Type t) => 
            string.Join(",", t.GetProperties().Select(x => x.Name + "=@" + x.Name));

        private static string GetInsertFields(Type t) =>
            $"({string.Join(",", t.GetProperties().Select(x => x.Name))}) VALUES ({string.Join(",", t.GetProperties().Select(x => "@" + x.Name))})";

        private static string GetMySqlLastInsertedId() => 
            "SELECT LAST_INSERT_ID()";
        private static string GetSqlLastInsertedId() => 
            "SELECT SCOPE_IDENTITY()";
    }
}
