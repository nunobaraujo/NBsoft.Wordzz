using Dapper;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Repositories;
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
        private readonly Func<IDbConnection> _createdDbConnection;
        private readonly ILogger _log;

        public SessionRepository(Func<IDbConnection> createdDbConnection, ILogger log)
        {
            _createdDbConnection = createdDbConnection;
            _log = log;
        }

        public async Task<IUserSession> Get(string token)
        {
            try
            {
                var res = new List<IUserSession>();
                var query = $"SELECT * FROM UserSession WHERE SessionToken = @SessionToken";
                using (var cnn = _createdDbConnection())
                {
                    return await cnn.QueryFirstOrDefaultAsync<UserSession>(query, new { SessionToken = token });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public async Task New(IUserSession session)
        {
            try
            {
                if (session == null)
                    throw new ArgumentException("Session cannot be null", nameof(session));
                var query = "INSERT INTO UserSession (SessionToken,UserId,UserInfo,Registered,LastAction,ActiveCompany) VALUES (@SessionToken,@UserId,@UserInfo,@Registered,@LastAction,@ActiveCompany)";
                using (var cnn = _createdDbConnection())
                {
                    var res = await cnn.ExecuteAsync(query, session);
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
                var query = $"DELETE FROM UserSession WHERE SessionToken=@SessionToken";
                using (var cnn = _createdDbConnection())
                {
                    var res = await cnn.ExecuteAsync(query, new { SessionToken = token });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public async Task Update(IUserSession session)
        {
            try
            {
                if (session == null)
                    throw new ArgumentException("UserId cannot be null", nameof(session));

                var fields = string.Join(",", typeof(IUserSession)
                    .GetProperties()
                    .Select(x => x.Name + "=@" + x.Name));
                fields = fields.Replace("SessionToken=@SessionToken,", "");
                var query = $"UPDATE UserSession SET {fields} WHERE SessionToken=@SessionToken";

                using (var cnn = _createdDbConnection())
                {
                    var res = await cnn.ExecuteAsync(query, session);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }
    }
}
