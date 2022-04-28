using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Cache;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.DictionaryApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    internal class CacheLexiconService : ILexiconService
    {
        private readonly static object padlock = new object();
        private readonly IWordRepository wordRepository;
        private readonly ILogger logger;
        private readonly DictionaryApiFactory dictionaryApiFactory;

        private DateTime lastCacheUpdate;
        private IEnumerable<ILexicon> lexicons;
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

        public Task<IWord> GetWordInfo(string language, string word)
        {
            Task.WaitAll(EnsureCacheIsUpdated());

            var result = DictionaryCache.Instance.GetWord(language, word);
            if (result == null)
                return null;

            lock (padlock)
            {
                if (string.IsNullOrEmpty(result.Description))
                {
                    var newDescritiptionTask = dictionaryApiFactory
                     .CreateDictionaryApi(new System.Globalization.CultureInfo(result.Language))
                     .GetDescription(result.Name);
                    Task.WaitAll(newDescritiptionTask);
                    var newDescritiption = newDescritiptionTask.Result;

                    logger.Info($"Aquired new word from API: [{result.Name}] = [{newDescritiption}]");
                    var newWord = new Word
                    {
                        Id = result.Id,
                        Language = result.Language,
                        Description = newDescritiption,
                        Name = result.Name
                    };

                    Task.WaitAll(wordRepository.Update(newWord));
                    DictionaryCache.Instance.AddOrUpdateWord(newWord);

                    result = newWord;
                }
                return Task.FromResult(result);
            }
        }

        public Task<bool> ValidateWord(string language, string word)
        {
            var result = DictionaryCache.Instance.GetWord(language, word);
            return Task.FromResult(result != null);
        }

        private async Task EnsureCacheIsUpdated()
        {
            if (DateTime.UtcNow > lastCacheUpdate.AddMinutes(5))
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
