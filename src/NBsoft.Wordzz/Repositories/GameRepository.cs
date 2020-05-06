using Dapper;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts;
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
    internal class GameRepository : IGameRepository
    {
        private readonly ILogger log;
        private readonly Func<IDbConnection> createdDbConnection;
        private readonly Func<Type, string> getSqlUpdateFields;
        private readonly Func<Type, string> getSqlInsertFields;


        public GameRepository(ILogger log, Func<IDbConnection> createdDbConnection,
            Func<Type, string> getSqlUpdateFields, Func<Type, string> getSqlInsertFields)
        {
            this.createdDbConnection = createdDbConnection;
            this.getSqlUpdateFields = getSqlUpdateFields;
            this.getSqlInsertFields = getSqlInsertFields;
            this.log = log;
        }
        public async Task<GameDataModel> Add(GameDataModel game)
        {
            try
            {
                if (game == null)
                    throw new ArgumentNullException(nameof(game));

                using var cnn = createdDbConnection();
                cnn.Open();
                var transaction = cnn.BeginTransaction();
                // Validate username
                var userId = await cnn.ExecuteScalarAsync($"SELECT Id FROM Game WHERE Id=@Id",
                    new { game.Id }, transaction);
                if (userId != null)
                    throw new InvalidConstraintException($"Game {game.Id} already exist.");


                // Create Game
                string query = $"INSERT INTO Game {getSqlInsertFields(typeof(GameDataModel))}";
                var res = await cnn.ExecuteAsync(query, game, transaction);
                if (res == 0)
                    throw new Exception($"ExecuteAsync failed: {query} [{game.ToJson()}]");

                // Create Game Moves


                transaction.Commit();
                return game;
            }
            catch (Exception ex)
            {
                await log?.ErrorAsync("Error inserting game", ex);
                throw;
            }
        }

        public async Task<int> AddMoves(string gameId, IEnumerable<GameMoveDataModel> moves)
        {
            try
            {
                if (string.IsNullOrEmpty(gameId))
                    throw new ArgumentNullException(nameof(gameId));
                if (moves == null || moves.Count()<1)
                    throw new ArgumentNullException(nameof(moves));


                using var cnn = createdDbConnection();
                cnn.Open();                
                // Validate game
                var game = await Get(gameId);
                if (game == null)
                    throw new ArgumentOutOfRangeException($"Game {game.Id} doesn't exist.");

                foreach (var move in moves)
                {
                    move.GameId = game.Id;
                }

                // Create Moves
                string query = $"INSERT INTO GameMove {getSqlInsertFields(typeof(GameMoveDataModel))}";
                var res = await cnn.ExecuteAsync(query, moves);
                if (res != moves.Count())
                    throw new Exception($"ExecuteAsync failed: {query}");
                
                return res;
            }
            catch (Exception ex)
            {
                await log?.ErrorAsync("Error inserting game moves", ex);
                throw;
            }
        }

        public async Task<IEnumerable<GameDataModel>> GetByUser(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));

                using var cnn = createdDbConnection();
                var query = @"SELECT * FROM Game WHERE Player01 = @userName OR Player02 = @userName";
                return await cnn.QueryAsync<GameDataModel>(query, new { userName });
            }
            catch (Exception ex)
            {
                await log?.ErrorAsync($"Error getting game by user: [{userName}]", ex);
                throw;
            }
        }

        public async Task<GameDataModel> Get(string gameId)
        {
            try
            {
                if (string.IsNullOrEmpty(gameId))
                    throw new ArgumentNullException(nameof(gameId));

                using var cnn = createdDbConnection();
                var query = @"SELECT * FROM Game WHERE Id = @gameId";
                return (await cnn.QueryAsync<GameDataModel>(
                    query, new { gameId }))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                await log?.ErrorAsync("Error getting game", ex);
                throw;
            }
        }

        public async Task<IEnumerable<GameDataModel>> GetActive()
        {
            try
            {   
                using var cnn = createdDbConnection();
                var query = @"SELECT * FROM Game WHERE Status = @status";
                return await cnn.QueryAsync<GameDataModel>(query, new { status = (int)GameStatus.Ongoing });
            }
            catch (Exception ex)
            {
                await log?.ErrorAsync("Error getting active games.", ex);
                throw;
            }
        }

        public async Task<IEnumerable<GameMoveDataModel>> GetMoves(string gameId)
        {
            try
            {
                using var cnn = createdDbConnection();
                var query = @"SELECT * FROM GameMove WHERE GameId = @gameId";
                return await cnn.QueryAsync<GameMoveDataModel>(query, new { gameId });
            }
            catch (Exception ex)
            {
                await log?.ErrorAsync("Error getting active games.", ex);
                throw;
            }
        }

        public async Task<IEnumerable<GameMoveDataModel>> GetMovesByUser(string userId)
        {
            try
            {
                using var cnn = createdDbConnection();
                var query = @"SELECT * FROM GameMove WHERE PlayerId = @userId";
                return await cnn.QueryAsync<GameMoveDataModel>(query, new { userId });
            }
            catch (Exception ex)
            {
                await log?.ErrorAsync("Error getting active games.", ex);
                throw;
            }
        }

        public async Task<GameDataModel> Update(GameDataModel game)
        {
            try
            {
                if (game == null || game.Id == null)
                    throw new ArgumentNullException(nameof(game));
                var existing = await Get(game.Id);
                if (existing == null)
                    throw new Exception($"Game doesn't exist: {game.Id}");


                using var cnn = createdDbConnection();
                cnn.Open();
                string query = $"UPDATE Game SET {getSqlUpdateFields(typeof(GameDataModel))}"
                    .Replace("Id=@Id,", "");
                query += " WHERE Id=@Id";

                if (await cnn.ExecuteAsync(query, game) != 1)
                    throw new Exception($"ExecuteAsync failed: {query}");

                return game;
            }
            catch (Exception ex)
            {
                log?.ErrorAsync("Error updating user stats", ex);
                throw;
            }
        }
    }
}
