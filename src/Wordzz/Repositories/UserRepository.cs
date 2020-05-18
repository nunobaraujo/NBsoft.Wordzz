using Dapper;
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
    internal class UserRepository : IUserRepository
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
        }
                
        public async Task<IUser> Add(IUser user,string email)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                if (!email.IsValidEmail())
                    throw new ArgumentException("Invalid email");

                using var cnn = _createdDbConnection();
                cnn.Open();
                var transaction = cnn.BeginTransaction();
                // Validate username
                var userId = await cnn.ExecuteScalarAsync($"SELECT UserName FROM User WHERE UserName=@UserName", 
                    new { user.UserName }, transaction);
                if (userId != null)
                    throw new InvalidConstraintException($"Username already exists: {user.UserName}");
                
                // Validate email
                var userEmail = await cnn.ExecuteScalarAsync($"SELECT UserName FROM UserDetails WHERE Email=@Email", 
                    new { Email = email.EncryptField(_encryptionKey) }, transaction);
                if (userEmail != null)
                    throw new InvalidConstraintException($"Email already exists: {email}");


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

                // Create user details
                query = $"INSERT INTO UserDetails {_getSqlInsertFields(typeof(UserDetails))}";
                var details = new UserDetails { UserName = user.UserName, Email = email };
                res = await cnn.ExecuteAsync(query, details.Encrypt(_encryptionKey), transaction);
                if (res == 0)
                    throw new Exception($"ExecuteAsync failed: {query} [{user.ToJson()}]");

                transaction.Commit();

                var settings = new UserSettings
                {
                    UserName = user.UserName,
                    MainSettings = new MainSettings { UserId = user.UserName, DefaultBoard = 1, Language="en-US"}.ToJson(),
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

                var user = await this.FindUser(userName);
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
        public async Task<IUser> GetByEmail(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    throw new ArgumentNullException(nameof(email));

                using var cnn = _createdDbConnection();
                var userName = await cnn.ExecuteScalarAsync<string>($"SELECT UserName FROM UserDetails WHERE Email=@Email",
                    new { Email = email.EncryptField(_encryptionKey) });
                if (userName == null)
                    return null;

                return await Get(userName);
            }
            catch (Exception ex)
            {
                await _log?.WriteErrorAsync(nameof(UserRepository), nameof(GetByEmail), email, null, ex);
                throw;
            }
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


        

        public async Task<string> AddContact(string userName, string contactUserName)
        {
            try
            {
                if (userName == null)
                    throw new ArgumentNullException(nameof(userName));

                if (contactUserName == null)
                    throw new ArgumentNullException(nameof(contactUserName));

                if (userName == contactUserName) 
                {
                    throw new ArgumentException();
                }
                // Validate if user EXISTS
                var user = await Get(userName);
                if (user == null)
                    throw new ArgumentException($"User doesn't exist: {userName}");

                // Validate if contact EXISTS
                var newContact = await Get(contactUserName);
                if (newContact == null)
                    throw new ArgumentException($"Contact doesn't exist: {contactUserName}");

                using var cnn = _createdDbConnection();
                cnn.Open();
                var transaction = cnn.BeginTransaction();
                

                // Validate if contact is already on list
                var userId = await cnn.ExecuteScalarAsync($"SELECT Contact FROM UserContacts WHERE UserName=@UserName AND Contact=@Contact",
                    new { UserName = userName, Contact  = contactUserName }, transaction);
                if (userId != null)
                    throw new InvalidConstraintException($"Contact already exists: {contactUserName}");


                // Create Contact
                string query = $"INSERT INTO UserContacts (UserName, Contact) VALUES (@UserName, @Contact)";
                var res = await cnn.ExecuteAsync(query, new { UserName = userName, Contact = contactUserName }, transaction);
                if (res == 0)
                    throw new Exception($"ExecuteAsync failed: {query} [{userName}],[{contactUserName}]");

                transaction.Commit();

                return newContact.UserName;
            }
            catch (Exception ex)
            {
                await _log?.WriteErrorAsync(nameof(UserRepository), nameof(AddContact), userName, null, ex);
                throw;
            }
        }

        public async Task<bool> DeleteContact(string userName, string contactUserName)
        {
            try
            {
                if (userName == null)
                    throw new ArgumentNullException(nameof(userName));

                if (contactUserName == null)
                    throw new ArgumentNullException(nameof(contactUserName));

                // Validate if user EXISTS
                var user = await Get(userName);
                if (user == null)
                    return false;

                // Validate if contact EXISTS
                var newContact = await Get(contactUserName);
                if (newContact == null)
                    return false;

                using var cnn = _createdDbConnection();
                cnn.Open();
                var transaction = cnn.BeginTransaction();


                // Validate if contact is already on list
                var userId = await cnn.ExecuteScalarAsync($"SELECT Contact FROM UserContacts WHERE UserName=@UserName AND Contact=@Contact",
                    new { UserName = userName, Contact = contactUserName }, transaction);
                if (userId == null)
                    return false;


                // Create Contact
                string query = "DELETE FROM UserContacts WHERE UserName=@UserName AND Contact=@Contact";
                var res = await cnn.ExecuteAsync(query, new { UserName = userName, Contact = contactUserName }, transaction);
                if (res == 0)
                    throw new Exception($"ExecuteAsync failed: {query} [{userName}],[{contactUserName}]");

                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                await _log?.WriteErrorAsync(nameof(UserRepository), nameof(AddContact), userName, null, ex);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetContacts(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));

                using var cnn = _createdDbConnection();
                var query = @"SELECT Contact FROM UserContacts WHERE UserName=@UserName";
                return await cnn.QueryAsync<string>(
                    query, new { UserName = userName });
            }
            catch (Exception ex)
            {
                await _log?.WriteErrorAsync(nameof(UserRepository), nameof(Get), userName, null, ex);
                throw;
            }
        }
    }
}
