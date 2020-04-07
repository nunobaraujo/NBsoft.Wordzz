using Dapper;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Extensions;
using NBsoft.Wordzz.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Repositories
{
    class UserRepository : IUserRepository
    {
        private static class SqlCommands
        {
            public const string SELECT = "SELECT * FROM User";
        }

        private readonly ILogger _log;
        private readonly Func<IDbConnection> _createdDbConnection;
        private readonly Func<Type, string> _getSqlUpdateFields;
        private readonly string _encryptionKey;

        public UserRepository(ILogger log, Func<IDbConnection> createdDbConnection, Func<Type, string> getSqlUpdateFields, string encryptionKey)
        {
            _createdDbConnection = createdDbConnection;
            _getSqlUpdateFields = getSqlUpdateFields;
            _log = log;
            _encryptionKey = encryptionKey;
        }

        public Task<IUser> Add(IUser user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                using (var cnn = _createdDbConnection())
                {
                    cnn.Open();
                    var transaction = cnn.BeginTransaction();
                    // Validate username
                    var userId = await cnn.ExecuteScalarAsync(
                        $"SELECT UserName FROM User WHERE UserName=@UserName", new { user.UserName }, transaction);
                    if (userId != null)
                        throw new InvalidConstraintException($"Username already exists: {user.UserName}");

                    var encrypted = user.EncryptUser(_getEncryptionKey());
                    var res = await cnn.ExecuteAsync(SqlCommands.INSERT, encrypted, transaction);
                    if (res == 0)
                        throw new Exception($"ExecuteAsync failed: {SqlCommands.INSERT}");

                    res = await cnn.ExecuteAsync(SqlCommands.INSERTUserDetails, encrypted, transaction);
                    if (res == 0)
                        throw new Exception($"ExecuteAsync failed: {SqlCommands.INSERT}");


                    transaction.Commit();

                    return user;
                }
            }
            catch (Exception ex)
            {
                _log?.WriteError(nameof(UserCommands), nameof(Add), user?.ToJson(), null, ex);
                throw;
            }
        }

        public async Task<IUser> Auth(string userName, string userPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));
                if (string.IsNullOrEmpty(userPassword))
                    throw new ArgumentNullException(nameof(userPassword));

                var user = await Get(userName);
                if (user == null)
                    user = await GetByEmail(userName);
                if (user == null)
                    return null;

                var isValid = user.CheckPassword(userPassword, _encryptionKey);
                return isValid ? user : null;
            }
            catch (Exception ex)
            {
                _log?.WriteError(nameof(UserRepository), nameof(Auth), userName, null, ex);
                throw;
            }
        }

        public Task Delete(string userName)
        {
            throw new NotImplementedException();
        }

        
        public async Task<IUser> Get(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));

                using (var cnn = _createdDbConnection())
                {
                    var query = $@"{SqlCommands.SELECT}
                        WHERE UserName = @UserName";
                    return (await cnn.QueryAsync<User>(
                        query, new { UserName = userName }))
                        .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _log?.WriteError(nameof(UserRepository), nameof(Get), userName, null, ex);
                throw;
            }
        }

        public Task<IUser> GetByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IUser>> List()
        {
            throw new NotImplementedException();
        }

        public async Task SetPassword(string userId, string password)
        {
            try
            {
                var user = await Get(userId);
                if (user != null)
                {
                    await Update(user.SetPassword(password, _encryptionKey));
                }
            }
            catch (Exception ex)
            {
                _log?.WriteError(nameof(UserRepository), nameof(SetPassword), userId, null, ex);
                throw;
            }
        }

        public Task<IUser> Update(IUser user)
        {
            throw new NotImplementedException();
        }
    }
}
