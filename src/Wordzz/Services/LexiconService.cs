using Microsoft.Extensions.Options;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.DictionaryApi;
using NBsoft.Wordzz.Extensions;
using NBsoft.Wordzz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    internal class LexiconService : ILexiconService
    {
        private static object dictionaryLock = new object();
        private static object apiLock = null;

        private readonly IWordRepository wordRepository;
        private readonly ILogger logger;        
        private readonly DictionaryApiFactory dictionaryApiFactory;
        private readonly bool useCache;

        private Dictionary<string, IEnumerable<string>> dictionaries;
        private IEnumerable<ILexicon> availableDictionaries;

        public LexiconService(ILogger logger, IWordRepository wordRepository, IOptions<WordzzSettings> settings, DictionaryApiFactory dictionaryApiFactory)
        {
            this.logger = logger;
            this.wordRepository = wordRepository;

            var s = settings.Value;
            useCache = s.UseLexiconCache;
            availableDictionaries = new List<ILexicon>();

            this.dictionaryApiFactory = dictionaryApiFactory;

            if (useCache)
                Task.Run(async () => await Initialize()).Wait();
            else
                logger.Info("Not using Lexicon Cache!");
        }

        private async Task Initialize()
        {
            dictionaries = new Dictionary<string, IEnumerable<string>>();
            await logger.InfoAsync("Initializing Lexicon Cache...");            
            var languages = await AvailableLexicons();
            foreach (var l in languages)
            {
                await LoadDictionary(l.Language);
            }
            await logger.InfoAsync("Lexicon Cache Initialized!");
        }

        private static object ApiLock { 
            get 
            {
                if (apiLock == null)
                    apiLock = new object();
                return apiLock;
            }
        }


        public async Task<ILexicon> GetDictionary(string language)
        {
            var isLanguageValid = await ValidateLanguage(language);
            if (!isLanguageValid)
                return null;
            return availableDictionaries.Single(d => d.Language == language);
        }
        public async Task<IWord> GetWord(string language, string word)
        {
            bool isLanguageValid = await ValidateLanguage(language);
            if (!isLanguageValid)
                return null;
            
            return await wordRepository.Get(language, word);
        }
        public Task<IWord> GetWordInfo(string language, string word)
        {
            lock (ApiLock)
            {
                var wordResult = GetWord(language, word).Result;
                if (wordResult == null)
                    return Task.FromResult<IWord>(null);

                // get info from word API and save to DB
                if (string.IsNullOrEmpty(wordResult.Description))
                    return DownloadWordInfo(wordResult);
                return Task.FromResult(wordResult);
            }
            
        }
                               
                
        public async Task LoadDictionary(string language)
        {            
            bool added = false;
            lock (dictionaryLock)
            {
                if (!dictionaries.ContainsKey(language))
                {
                    dictionaries.Add(language, null);
                    added = true;
                }
            }
            if (added)
            {
                var dictionary = await wordRepository.ListWords(language);
                await logger.InfoAsync($"Loaded {language} dictionary: {dictionary.Count()} words");
                dictionaries[language] = dictionary;
            }
        }

        public async Task<bool> ValidateWord(string language, string word)
        {
            var upperWord = word.ToUpper();
            // Validate Language
            bool isLanguageValid = await ValidateLanguage(language);
            if (!isLanguageValid)                         
                return false;

            var isValid = false;
            
            if (useCache)
            {
                // Load Dictionary if not loaded yet
                await LoadDictionary(language);
                lock (dictionaryLock)
                {
                    isValid = dictionaries[language].Any(w => w == upperWord);
                }
            }
            else
            {
                isValid = (await wordRepository.Get(language, upperWord)) != null;
            }           
            return isValid;
        }
        private async Task<bool> ValidateLanguage(string language)
        {
            // if language doesnt exist reload from db
            if (!availableDictionaries.Any(d => d.Language == language))
                availableDictionaries = await wordRepository.ListDictionaries();

            var result = availableDictionaries.Any(d => d.Language == language);
            if (!result)
                await logger.WarningAsync($"Invalid language: {language}");
            return result;
        }
        
        private async Task<IWord> DownloadWordInfo(IWord word)
        {
            string newDescritiption;
            try
            {
                var culture = new System.Globalization.CultureInfo(word.Language);
                newDescritiption = await dictionaryApiFactory
                    .CreateDictionaryApi(culture)
                    .GetDescription(word.Name);
                await logger.InfoAsync($"Aquired new word from API: [{word.Name}] = [{newDescritiption}]");
            }
            catch (Exception ex)
            {
                newDescritiption = null;
                await logger.ErrorAsync("Error contacting dictionary API", ex);                
            }

            var editable = word.ToDto<Word>();
            if (!string.IsNullOrEmpty(newDescritiption))
            {                
                editable.Description = newDescritiption;
                await wordRepository.Update(editable);
                return editable;
            }
            editable.Description = editable.Name;
            return editable;

        }
        

        public async Task<IEnumerable<ILexicon>> AvailableLexicons()
        {
            return await wordRepository.ListDictionaries();
        }
    }
}
