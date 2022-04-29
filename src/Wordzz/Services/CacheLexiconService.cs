using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Cache;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.DictionaryApi;
using NBsoft.Wordzz.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    internal class CacheLexiconService : ILexiconService
    {
        private static object padLock = new object();
        private readonly IWordRepository wordRepository;
        private readonly ILogger logger;
        private readonly DictionaryApiFactory dictionaryApiFactory;

        private DateTime lastCacheUpdate;
        private IEnumerable<ILexicon> lexicons;
        private const int CacheDurationMinutes = 60;

        public CacheLexiconService(ILogger logger, IWordRepository wordRepository, DictionaryApiFactory dictionaryApiFactory)
        {
            this.logger = logger;
            this.wordRepository = wordRepository;
            this.dictionaryApiFactory = dictionaryApiFactory;

            Task.Run(async () => await RefreshCache()).Wait();
        }

        public async Task<ILexicon> GetDictionary(string language)
        {
            await EnsureCacheIsUpdated();
            return lexicons.SingleOrDefault(x => x.Language.ToUpper() == language.ToUpper());
        }

        public async Task<IWord> GetWordInfo(string language, string word)
        {
            await EnsureCacheIsUpdated();
            // Lock to prevent concurrent requests of the same word to the Dictionary API
            // If the word has no description, when the lock is over it should already have it.
            // next call in queue will not get an empty description on GetWord
            lock (padLock) 
            {
                var result = DictionaryCache.Instance.GetWord(language, word);
                if (result == null)
                    return null;

                if (string.IsNullOrEmpty(result.Description))
                {
                    var newWord = dictionaryApiFactory.UpdateDescription(result);
                    DictionaryCache.Instance.AddOrUpdateWord(newWord);

                    logger.Info($"Aquired new word from API: [{newWord.Name}] = [{newWord.Description}]");

                    Task.WaitAll(wordRepository.Update(newWord));
                    result = newWord;
                }
                return result;
            }
        }

        public async Task<bool> ValidateWord(string language, string word)
        {
            await EnsureCacheIsUpdated();
            var result = DictionaryCache.Instance.GetWord(language, word);
            return result != null;
        }

        private async Task EnsureCacheIsUpdated()
        {
            if (DateTime.UtcNow > lastCacheUpdate.AddMinutes(CacheDurationMinutes))
                await RefreshCache();
        }

        private async Task RefreshCache()
        {   
            await logger.InfoAsync("Refreshing Lexicon Cache...");
            lexicons = await wordRepository.ListDictionaries();

            foreach (var l in lexicons.AsParallel())
            {
                var words = await wordRepository.GetAllWords(l.Language);
                DictionaryCache.Instance.AddOrUpdateDictionary(l.Language, words);

            }
            await logger.InfoAsync("Lexicon Cache Refreshed!");

            lastCacheUpdate = DateTime.UtcNow;
        }
        
    }
}
