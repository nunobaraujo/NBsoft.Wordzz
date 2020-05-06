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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Repositories
{
    internal class BoardRepository : IBoardRepository
    {
        private readonly ILogger _log;
        private readonly Func<IDbConnection> createdDbConnection;
        private readonly Func<string> getLastId;


        public BoardRepository(ILogger log, Func<IDbConnection> createdDbConnection, Func<string> getLastId)
        {
            this.createdDbConnection = createdDbConnection;
            this.getLastId = getLastId;
            _log = log;
        }

        public async Task<IBoard> Add(IBoard board)
        {
            try
            {
                if (board == null)
                    throw new ArgumentNullException(nameof(board));

                using var cnn = createdDbConnection();
                cnn.Open();
                var transaction = cnn.BeginTransaction();

                // Create board
                string query = $"INSERT INTO Board (Name,BoardRows,BoardColumns) VALUES (@Name,@BoardRows,@BoardColumns)";
                var res = await cnn.ExecuteAsync(query, board, transaction);
                if (res == 0)
                    throw new Exception($"ExecuteAsync failed: {query}");

                var lastId = await cnn.ExecuteScalarAsync<int>(getLastId(), transaction);

                // Force Tile foreign key
                var tiles = board.Tiles;
                var newTiles = new List<IBoardTile>();
                foreach (var tile in tiles)
                {
                    var dtoTile = tile.ToDto<BoardTile>();
                    dtoTile.BoardId = lastId;
                    newTiles.Add(dtoTile);
                }
                // Create Tiles
                query = $"INSERT INTO BoardTile (BoardId,X,Y,Bonus) VALUES (@BoardId,@X,@Y,@Bonus)";
                res = await cnn.ExecuteAsync(query, newTiles, transaction);
                if (res != board.Tiles.Length)
                    throw new Exception($"ExecuteAsync failed: {query}");

                transaction.Commit();
                return await Get(lastId);
            }
            catch (Exception ex)
            {
                await _log?.ErrorAsync("Error inserting board", ex);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                if (id < 0)
                    return false;

                var existing = Get(id);
                if (existing == null)
                    return false;

                using var cnn = createdDbConnection();
                cnn.Open();
                var trans = cnn.BeginTransaction();

                var query = "DELETE FROM BoardTile WHERE BoardId = @id";
                var result = await cnn.ExecuteAsync(query, new { id }, trans);
                
                query = "DELETE FROM Board WHERE Id = @id";
                result = await cnn.ExecuteAsync(query, new { id }, trans);
                trans.Commit();

                return result == 1;
            }
            catch (Exception ex)
            {
                await _log?.ErrorAsync("Error getting board", ex);
                throw;
            }
        }

        public async Task<IBoard> Get(int id)
        {
            try
            {
                if (id<0)
                    throw new ArgumentNullException(nameof(id));

                using var cnn = createdDbConnection();
                var query = "SELECT * FROM Board WHERE Id = @id";
                var board = (await cnn.QueryAsync<Board>(query, new { id }))
                    .FirstOrDefault();
                if (board == null)
                    return null;
                
                query = "SELECT * FROM BoardTile WHERE BoardId = @id";
                var tiles = await cnn.QueryAsync<BoardTile>(query, new { id });

                board.Tiles = tiles.ToArray();

                return board;
            }
            catch (Exception ex)
            {
                await _log?.ErrorAsync("Error getting board", ex);
                throw;
            }
        }

        public async Task<IEnumerable<IBoard>> List()
        {
            try
            {
                using var cnn = createdDbConnection();
                var query = "SELECT * FROM Board";
                return await cnn.QueryAsync<Board>(query);
            }
            catch (Exception ex)
            {
                await _log?.ErrorAsync("Error listing boards", ex);
                throw;
            }
        }

        public Task<IBoard> Update(IBoard game)
        {
            throw new NotImplementedException();
        }
    }
}
