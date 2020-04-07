using MySql.Data.MySqlClient;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core.Repositories;
using System.Data.SqlClient;
using System;

namespace NBsoft.Wordzz.Repositories
{
    internal static class RepositoryFactory
    {
        public enum RepositoryType 
        {
            MsSqlServer = 0,
            MySql = 1
        }

        public static ISessionRepository CreateSessionRepository(RepositoryType repositoryType, string connectionString, ILogger logger)
        {
            switch (repositoryType)
            {
                case RepositoryType.MsSqlServer:
                    return new SessionRepository(() => new SqlConnection(connectionString), logger);
                case RepositoryType.MySql:
                    return new SessionRepository(() => new MySqlConnection(connectionString), logger);
                default:
                    throw new ArgumentOutOfRangeException(nameof(repositoryType));

            }
            
        }

    }
}
