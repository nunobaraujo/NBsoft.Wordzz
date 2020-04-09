﻿using Dapper;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Contracts.Settings;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Entities;
using NBsoft.Wordzz.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Repositories
{
    class UserRepository : IUserRepository
    {
       
        private readonly ILogger _log;
        private readonly Func<IDbConnection> _createdDbConnection;
        private readonly Func<Type, string> _getSqlUpdateFields;
        private readonly Func<Type, string> _getSqlInsertFields;
        private readonly string _encryptionKey;

        public UserRepository(ILogger log, Func<IDbConnection> createdDbConnection, Func<Type, string> getSqlUpdateFields, Func<Type, string> getSqlInsertFields, string encryptionKey)
        {
            _createdDbConnection = createdDbConnection;
            _getSqlUpdateFields = getSqlUpdateFields;
            _getSqlInsertFields = getSqlInsertFields;
            _log = log;
            _encryptionKey = encryptionKey;

            CheckAdminUser();
        }
                
        public async Task<IUser> Add(IUser user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                using var cnn = _createdDbConnection();
                cnn.Open();
                var transaction = cnn.BeginTransaction();
                // Validate username
                var userId = await cnn.ExecuteScalarAsync($"SELECT UserName FROM User WHERE UserName=@UserName", new { user.UserName }, transaction);
                if (userId != null)
                    throw new InvalidConstraintException($"Username already exists: {user.UserName}");

                // Create User
                string query = $"INSERT INTO User {_getSqlInsertFields(typeof(User))}";
                var res = await cnn.ExecuteAsync(query, user, transaction);
                if (res == 0)
                    throw new Exception($"ExecuteAsync failed: {query} [{user.ToJson()}]");

                // Create User Settings                
                query = $"INSERT INTO UserSettings {_getSqlInsertFields(typeof(UserSettings))}";
                res = await cnn.ExecuteAsync(query, new UserSettings { UserName = user.UserName }, transaction);
                if (res == 0)
                    throw new Exception($"ExecuteAsync failed: {query} [{user.ToJson()}]");

                query = $"INSERT INTO UserDetails {_getSqlInsertFields(typeof(UserDetails))}";
                res = await cnn.ExecuteAsync(query, new UserDetails { UserName = user.UserName }, transaction);
                if (res == 0)
                    throw new Exception($"ExecuteAsync failed: {query} [{user.ToJson()}]");

                transaction.Commit();

                var settings = new UserSettings
                {
                    UserName = user.UserName,
                    MainSettings = new MainSettings { UserId = user.UserName }.ToJson(),
                    WindowsSettings = new WindowsSettings { UserId = user.UserName }.ToJson(),
                    AndroidSettings = new AndroidSettings { UserId = user.UserName }.ToJson(),
                    IOSSettings = new IOSSettings { UserId = user.UserName }.ToJson()
                };
                var addedSettings = await UpdateSettings(settings);

                return user;
            }
            catch (Exception ex)
            {
                await _log?.WriteErrorAsync(nameof(UserRepository), nameof(Add), user?.ToJson(), null, ex);
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
        public async Task<IUser> Update(IUser user)
        {
            try
            {
                if (user == null || user.UserName == null)
                    throw new ArgumentNullException(nameof(user));

                using var cnn = _createdDbConnection();
                cnn.Open();
                string query = $"UPDATE User SET {_getSqlUpdateFields(typeof(User))}"
                    .Replace("UserName=@UserName,", "");
                query += " WHERE UserName=@UserName";

                if (await cnn.ExecuteAsync(query, user) != 1)
                    throw new Exception($"ExecuteAsync failed: {query}");

                return user;
            }
            catch (Exception ex)
            {
                _log?.WriteError(nameof(UserRepository), nameof(Update), user?.ToJson(), null, ex);
                throw;
            }
        }
        public async Task Delete(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));

                var user = await Get(userName);
                if (user == null)
                    throw new ArgumentException($"User does not exist {userName}");

                using var cnn = _createdDbConnection();
                cnn.Open();
                var transaction = cnn.BeginTransaction();

                // Set user as deleted
                string query = "UPDATE User Set Deleted=1 WHERE UserName = @UserName";
                var res = await cnn.ExecuteAsync(query, new { UserName = userName }, transaction);
                if (res != 1)
                    throw new Exception($"ExecuteAsync failed: {query} [{user.ToJson()}]");

                // remove user settings
                query = "DELETE FROM UserSettings WHERE UserName = @UserName";
                res = await cnn.ExecuteAsync(query, new { UserName = userName }, transaction);
                if (res != 1)
                    throw new Exception($"ExecuteAsync failed: {query} [{user.ToJson()}]");

                // remove user details
                query = "DELETE FROM UserDetails WHERE UserName = @UserName";
                res = await cnn.ExecuteAsync(query, new { UserName = userName }, transaction);
                if (res != 1)
                    throw new Exception($"ExecuteAsync failed: {query} [{user.ToJson()}]");

                transaction.Commit();
            }
            catch (Exception ex)
            {
                await _log?.WriteErrorAsync(nameof(UserRepository), nameof(Delete), userName, null, ex);
                throw;
            }
        }
        
        public async Task<IUser> Get(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));

                using var cnn = _createdDbConnection();
                var query = @"SELECT * FROM User WHERE UserName = @UserName AND Deleted = 0";
                return (await cnn.QueryAsync<User>(
                    query, new { UserName = userName }))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                await _log?.WriteErrorAsync(nameof(UserRepository), nameof(Get), userName, null, ex);
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
                if (user == null)
                    throw new ArgumentException($"Invalid user id: {userId}");
                var updated = user.SetPassword(password, _encryptionKey);
                await Update(updated);
            }
            catch (Exception ex)
            {
                _log?.WriteError(nameof(UserRepository), nameof(SetPassword), userId, null, ex);
                throw;
            }
        }        

        public async Task<IUserDetails> GetDetails(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));

                using var cnn = _createdDbConnection();
                var query = @"SELECT * FROM UserDetails WHERE UserName = @UserName";
                var userDetails =  (await cnn.QueryAsync<UserDetails>(query, new { UserName = userName }))
                    .FirstOrDefault();
                if (userDetails == null)
                    return null;
                return userDetails.Decrypt(_encryptionKey);
            }
            catch (Exception ex)
            {
                await _log?.WriteErrorAsync(nameof(UserRepository), nameof(GetDetails), userName, null, ex);
                throw;
            }
        }
        public async Task<IUserDetails> UpdateDetails(IUserDetails userDetails)
        {
            try
            {
                if (userDetails == null || userDetails.UserName == null)
                    throw new ArgumentNullException(nameof(userDetails));

                using var cnn = _createdDbConnection();
                cnn.Open();
                string query = $"UPDATE UserDetails SET {_getSqlUpdateFields(typeof(UserDetails))}"
                    .Replace("UserName=@UserName,", "");
                query += " WHERE UserName=@UserName";

                if (await cnn.ExecuteAsync(query, userDetails.Encrypt(_encryptionKey)) != 1)
                    throw new Exception($"ExecuteAsync failed: {query}");

                return userDetails;
            }
            catch (Exception ex)
            {
                _log?.WriteError(nameof(UserRepository), nameof(UpdateDetails), userDetails.UserName, null, ex);
                throw;
            }
        }

        public async Task<IUserSettings> GetSettings(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));

                using var cnn = _createdDbConnection();
                var query = @"SELECT * FROM UserSettings WHERE UserName = @UserName";
                return (await cnn.QueryAsync<UserSettings>(
                    query, new { UserName = userName }))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                await _log?.WriteErrorAsync(nameof(UserRepository), nameof(Get), userName, null, ex);
                throw;
            }
        }
        public async Task<IUserSettings> UpdateSettings(IUserSettings userSettings)
        {
            try
            {
                if (userSettings == null || userSettings.UserName == null)
                    throw new ArgumentNullException(nameof(userSettings));

                using var cnn = _createdDbConnection();
                cnn.Open();
                string query = $"UPDATE UserSettings SET {_getSqlUpdateFields(typeof(UserSettings))}"
                    .Replace("UserName=@UserName,", "");
                query += " WHERE UserName=@UserName";

                if (await cnn.ExecuteAsync(query, userSettings) != 1)
                    throw new Exception($"ExecuteAsync failed: {query}");

                return userSettings;
            }
            catch (Exception ex)
            {
                _log?.WriteError(nameof(UserRepository), nameof(UpdateSettings), userSettings.UserName, null, ex);
                throw;
            }
        }


        private async Task CheckAdminUser()
        {
            var admin = await Get("sa");
            if (admin != null)
                return;

            var user = new User
            {
                UserName = "sa",
                CreationDate = DateTime.UtcNow,
                Deleted = false
            };
            var withPassword = user.SetPassword("#Na123@10", _encryptionKey);
            var added = await Add(withPassword);

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
            var addedDetails = await UpdateDetails(details);

            var settings = new UserSettings
            {
                UserName = addedDetails.UserName,
                MainSettings = new MainSettings { UserId = addedDetails.UserName }.ToJson(),
                WindowsSettings = new WindowsSettings { UserId = addedDetails.UserName }.ToJson(),
                AndroidSettings = new AndroidSettings { UserId = addedDetails.UserName }.ToJson(),
                IOSSettings = new IOSSettings { UserId = addedDetails.UserName }.ToJson()
            };
            var addedSettings = await UpdateSettings(settings);
            await _log.WarningAsync("Added server admin user");

        }
    }
}