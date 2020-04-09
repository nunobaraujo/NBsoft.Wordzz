using Dapper;
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
    public class WordRepository : IWordRepository
    {
        private readonly ILogger _log;
        private readonly Func<IDbConnection> _createdDbConnection;
        private readonly Func<Type, string> _getSqlUpdateFields;
        private readonly Func<Type, string> _getSqlInsertFields;
        private readonly Func<string> _getLastInsertedId;

        public WordRepository(ILogger log, 
            Func<IDbConnection> createdDbConnection, 
            Func<Type, string> getSqlUpdateFields, 
            Func<Type, string> getSqlInsertFields,
            Func<string> getLastInsertedId)
        {
            _createdDbConnection = createdDbConnection;
            _getSqlUpdateFields = getSqlUpdateFields;
            _getSqlInsertFields = getSqlInsertFields;
            _getLastInsertedId = getLastInsertedId;
            _log = log;            
        }

        public async Task<IWord> Add(IWord word)
        {
            try
            {
                if (word == null)
                    throw new ArgumentNullException(nameof(word));

                using var cnn = _createdDbConnection();
                cnn.Open();
                var transaction = cnn.BeginTransaction();

                // Check if Word exists in dictionary
                string checkQuery = "SELECT Name FROM Word WHERE Language=@Language AND Name=@Name";
                var existing = await cnn.ExecuteScalarAsync(checkQuery, new { word.Language, word.Name }, transaction);
                if (existing != null)
                    throw new InvalidConstraintException($"Word [{word}] already exists in dictionary {word.Language}.");

                // Create Word
                string query = $"INSERT INTO Word {_getSqlInsertFields(typeof(Word))}"
                    .Replace("@Id,", "")
                    .Replace("Id,", "");

                var res = await cnn.ExecuteAsync(query, word);
                if (res == 0)
                    throw new Exception($"ExecuteAsync failed: {query} [{word.ToJson()}]");


                var added = await cnn.ExecuteScalarAsync<uint>(_getLastInsertedId(), transaction);
                transaction.Commit();

                return await Get(added);
            }
            catch (Exception ex)
            {
                await _log?.WriteErrorAsync(nameof(WordRepository), nameof(Add), word?.ToJson(), null, ex);
                throw;
            }
        }

        public async Task<bool> AddDictionary(ILexicon lexicon, IEnumerable<IWord> words)
        {
            try
            {
                if (lexicon == null)
                    throw new ArgumentNullException(nameof(lexicon));
                
                using var cnn = _createdDbConnection();
                cnn.Open();
                var transaction = cnn.BeginTransaction();

                // Check if Lexicon exists
                string checkQuery = "SELECT Language FROM Lexicon WHERE Language=@Language";
                var existing = await cnn.ExecuteScalarAsync(checkQuery, new { lexicon.Language}, transaction);
                if (existing != null)
                    throw new InvalidConstraintException($"Dictionry [{lexicon.Language}] already exists");

                // Create User
                string query = $"INSERT INTO Lexicon {_getSqlInsertFields(typeof(Lexicon))}";

                var res = await cnn.ExecuteAsync(query, lexicon);
                if (res == 0)
                    throw new Exception($"ExecuteAsync failed: {query} [{lexicon.ToJson()}]");

                // Create Words
                string wordQuery = $"INSERT INTO Word {_getSqlInsertFields(typeof(Word))}"
                    .Replace("@Id,", "")
                    .Replace("Id,", "");

                var resWords = await cnn.ExecuteAsync(wordQuery, words);
                if (resWords != words.Count())
                    throw new Exception($"ExecuteAsync failed: {wordQuery}");
                                
                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                await _log?.WriteErrorAsync(nameof(WordRepository), nameof(AddDictionary), lexicon?.ToJson(), null, ex);
                return false;
            }
        }

        public Task<bool> Delete(IWord word)
        {
            throw new NotImplementedException();
        }

        public Task<IWord> Get(ILexicon lexicon, string word)
        {
            throw new NotImplementedException();
        }

        public Task<IWord> Get(uint wordId)
        {
            throw new NotImplementedException();
        }

        public async Task<ILexicon> GetDictionary(string language)
        {
            try
            {
                if (string.IsNullOrEmpty(language))
                    throw new ArgumentNullException(nameof(language));

                using var cnn = _createdDbConnection();
                var query = @"SELECT * FROM Lexicon WHERE Language = @Language";
                return (await cnn.QueryAsync<Lexicon>(query, new { Language = language }))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                await _log?.WriteErrorAsync(nameof(UserRepository), nameof(ListWords), null, null, ex);
                throw;
            }
        }

        public Task<IEnumerable<ILexicon>> ListDictionaries()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<string>> ListWords(ILexicon lexicon)
        {
            try
            {
                if (lexicon == null)
                    throw new ArgumentNullException(nameof(lexicon));

                using var cnn = _createdDbConnection();
                var query = @"SELECT Name FROM Word WHERE Language = @Language";
                return await cnn.QueryAsync<string>(
                    query, new { lexicon.Language });
            }
            catch (Exception ex)
            {
                await _log?.WriteErrorAsync(nameof(UserRepository), nameof(ListWords), null, null, ex);
                throw;
            }
        }

        public Task<IWord> Update(IWord word)
        {
            throw new NotImplementedException();
        }
    }
}
