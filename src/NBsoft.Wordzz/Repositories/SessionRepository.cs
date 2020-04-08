using Dapper;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Entities;
using NBsoft.Wordzz.Extensions;
using NBsoft.Wordzz.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Repositories
{
    class SessionRepository : ISessionRepository
    {
        private readonly ILogger _log;
        private readonly Func<IDbConnection> _createdDbConnection;
        private readonly Func<Type, string> _getSqlUpdateFields;

        public SessionRepository(ILogger log, Func<IDbConnection> createdDbConnection, Func<Type, string> getSqlUpdateFields)
        {
            _createdDbConnection = createdDbConnection;
            _getSqlUpdateFields = getSqlUpdateFields;
            _log = log;
        }

        private static class SqlCommands 
        {
            public const string SELECT = "SELECT * FROM Session";
            public const string INSERT =
                @"INSERT INTO Session 
                    (SessionToken,UserId,UserInfo,Registered,LastAction)
                    VALUES
                    (@SessionToken,@UserId,@UserInfo,@Registered,@LastAction)";
            public const string DELETE = "DELETE FROM Session WHERE SessionToken=@SessionToken";

            public const string INSERTHISTORY = @"INSERT INTO SessionHistory 
                                (SessionToken,UserId,UserInfo,Registered,LastAction,Expired) 
                                VALUES 
                                (@SessionToken,@UserId,@UserInfo,@Registered,@LastAction,@Expired)";
        }

        public async Task<ISession> Get(string token)
        {
            try
            {
                var res = new List<ISession>();
                var query = SqlCommands.SELECT + " WHERE SessionToken = @SessionToken";
                using (var cnn = _createdDbConnection())
                {
                    return await cnn.QueryFirstOrDefaultAsync<Session>(query, new { SessionToken = token });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public async Task New(ISession session)
        {
            try
            {
                if (session == null)
                    throw new ArgumentException("Session cannot be null", nameof(session));                
                using (var cnn = _createdDbConnection())
                {
                    var res = await cnn.ExecuteAsync(SqlCommands.INSERT, session);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public async Task Remove(string token)
        {
            try
            {
                var session = await Get(token);

                using var cnn = _createdDbConnection();
                if (session != null)
                {
                    var editable = session.ToDto();
                    editable.Expired = DateTime.UtcNow;
                    var res = await cnn.ExecuteAsync(SqlCommands.INSERTHISTORY, editable);
                }                
                await cnn.ExecuteAsync(SqlCommands.DELETE, new { SessionToken = token });
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public async Task Update(ISession session)
        {
            try
            {
                if (session == null)
                    throw new ArgumentException("UserId cannot be null", nameof(session));

                var fields = _getSqlUpdateFields(typeof(Session))
                    .Replace("SessionToken=@SessionToken,", "");                
                var query = $"UPDATE Session SET {fields} WHERE SessionToken=@SessionToken";

                using var cnn = _createdDbConnection();
                var res = await cnn.ExecuteAsync(query, session);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public async Task<IEnumerable<ISession>> List()
        {
            try
            {
                using var cnn = _createdDbConnection();
                return await cnn.QueryAsync<Session>(SqlCommands.SELECT);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }
    }
}
