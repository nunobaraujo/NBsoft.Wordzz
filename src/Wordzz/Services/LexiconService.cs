using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.DictionaryApi;
using NBsoft.Wordzz.Extensions;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    internal class LexiconService : ILexiconService
    {
        private static object padLock = new object();

        private readonly IWordRepository wordRepository;
        private readonly ILogger logger;        
        private readonly DictionaryApiFactory dictionaryApiFactory;
       
        public LexiconService(ILogger logger, IWordRepository wordRepository, DictionaryApiFactory dictionaryApiFactory)
        {
            this.logger = logger;
            this.wordRepository = wordRepository;
            this.dictionaryApiFactory = dictionaryApiFactory;
        }

        public async Task<ILexicon> GetDictionary(string language)
        {
            return await wordRepository.GetDictionary(language);
        }

        public Task<IWord> GetWordInfo(string language, string word)
        {
            // Lock to prevent concurrent requests of the same word to the Dictionary API
            // If the word has no description, when the lock is over it should already have it.
            // next call in queue will not get an empty description on wordRepository.Get
            lock (padLock)
            {
                var getWordTask = wordRepository.Get(language, word);
                getWordTask.Wait();
                var result = getWordTask.Result;
                if (result == null)
                    return null;

                if (string.IsNullOrEmpty(result.Description))
                {
                    var newWord = dictionaryApiFactory.UpdateDescription(result);
                    logger.Info($"Aquired new word from API: [{newWord.Name}] = [{newWord.Description}]");

                    Task.WaitAll(wordRepository.Update(newWord));
                    result = newWord;
                }
                return Task.FromResult(result);
            }
        }

        public async Task<bool> ValidateWord(string language, string word)
        {
            var result = await wordRepository.Get(language, word);
            return result != null;
        }
    }
}
