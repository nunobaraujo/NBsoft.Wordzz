using Dapper;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Repositories
{
    internal class StatsRepository : IStatsRepository
    {
        private readonly ILogger _log;
        private readonly Func<IDbConnection> _createdDbConnection;
        private readonly Func<Type, string> _getSqlUpdateFields;
        private readonly Func<Type, string> _getSqlInsertFields;

        public StatsRepository(ILogger log, Func<IDbConnection> createdDbConnection, 
            Func<Type, string> getSqlUpdateFields, Func<Type, string> getSqlInsertFields)
        {
            _createdDbConnection = createdDbConnection;
            _getSqlUpdateFields = getSqlUpdateFields;
            _getSqlInsertFields = getSqlInsertFields;
            _log = log;

        }

        public async Task<IUserStats> Add(IUserStats userStats)
        {
            try
            {
                if (userStats == null)
                    throw new ArgumentNullException(nameof(userStats));
                
                using var cnn = _createdDbConnection();
                cnn.Open();
                var transaction = cnn.BeginTransaction();
                // Validate username
                var userId = await cnn.ExecuteScalarAsync($"SELECT UserName FROM UserStats WHERE UserName=@UserName",
                    new { userStats.UserName }, transaction);
                if (userId != null)
                    throw new InvalidConstraintException($"Stats for {userStats.UserName} already exist.");


                // Create User Stats
                string query = $"INSERT INTO UserStats {_getSqlInsertFields(typeof(UserStats))}";
                var res = await cnn.ExecuteAsync(query, userStats, transaction);
                if (res == 0)
                    throw new Exception($"ExecuteAsync failed: {query} [{userStats.ToJson()}]");

                transaction.Commit();
                return userStats;
            }
            catch (Exception ex)
            {
                await _log?.ErrorAsync("Error inserting user stats", ex);
                throw;
            }
        }

        public async Task Delete(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));

                var userStats = await Get(userName);
                if (userStats == null)
                    throw new ArgumentException($"Stats does not exist for {userName}");

                using var cnn = _createdDbConnection();
                cnn.Open();
                var transaction = cnn.BeginTransaction();
                             
                // remove user settings
                string query = "DELETE FROM UserStats WHERE UserName = @UserName";
                var res = await cnn.ExecuteAsync(query, new { UserName = userName }, transaction);
                if (res != 1)
                    throw new Exception($"ExecuteAsync failed: {query} [{userStats.ToJson()}]");              

                transaction.Commit();
            }
            catch (Exception ex)
            {
                await _log?.ErrorAsync("Error deleting user stats", ex);
                throw;
            }

        }

        public async Task<IUserStats> Get(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));

                using var cnn = _createdDbConnection();
                var query = @"SELECT * FROM UserStats WHERE UserName = @UserName";
                return (await cnn.QueryAsync<UserStats>(
                    query, new { UserName = userName }))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                await _log?.ErrorAsync("Error getting user stats", ex);
                throw;
            }
        }

        public async Task<IEnumerable<IUserStats>> GetHighScores()
        {
            try
            {
                using var cnn = _createdDbConnection();
                var query = @"SELECT * FROM UserStats ORDER BY HighScoreGame DESC";
                var res = await cnn.QueryAsync<UserStats>(query);
                if (res.Count() > 10)
                    return res.Take(10);
                else
                    return res;
            }
            catch (Exception ex)
            {
                await _log?.ErrorAsync("Error getting user stats", ex);
                throw;
            }
        }

        public async Task<IUserStats> Update(IUserStats userStats)
        {
            try
            {
                if (userStats == null || userStats.UserName == null)
                    throw new ArgumentNullException(nameof(userStats));

                using var cnn = _createdDbConnection();
                cnn.Open();
                string query = $"UPDATE UserStats SET {_getSqlUpdateFields(typeof(UserStats))}"
                    .Replace("UserName=@UserName,", "");
                query += " WHERE UserName=@UserName";

                if (await cnn.ExecuteAsync(query, userStats) != 1)
                    throw new Exception($"ExecuteAsync failed: {query}");

                return userStats;
            }
            catch (Exception ex)
            {
                _log?.ErrorAsync("Error updating user stats", ex);
                throw;
            }
        }
    }
}
