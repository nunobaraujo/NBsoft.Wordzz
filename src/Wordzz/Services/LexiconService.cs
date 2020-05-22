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
        private static object apiLock = new object();

        private readonly IWordRepository wordRepository;
        private readonly ILogger logger;
        private readonly Dictionary<string, IEnumerable<string>> dictionaries;
        private readonly DictionaryApiFactory dictionaryApiFactory;


        private IEnumerable<ILexicon> availableDictionaries;

        public LexiconService(ILogger logger, IWordRepository wordRepository, IOptions<WordzzSettings> settings, DictionaryApiFactory dictionaryApiFactory)
        {
            this.logger = logger;
            this.wordRepository = wordRepository;

            dictionaries = new Dictionary<string, IEnumerable<string>>();
            availableDictionaries = new List<ILexicon>();

            this.dictionaryApiFactory = dictionaryApiFactory;

            Task.Run(async () => await Initialize()).Wait();
        }

        private async Task Initialize()
        {            
            await logger.InfoAsync("Initializing LexiconService...");            
            var languages = await AvailableLexicons();
            foreach (var l in languages)
            {
                await LoadDictionary(l.Language);
            }
            await logger.InfoAsync("LexiconService Initialized!");
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
        public async Task<IWord> GetWordInfo(string language, string word)
        {
            var wordResult = await GetWord(language, word);
            if (wordResult == null)
                return null;

                        
            // get info from word API and save to DB
            if (string.IsNullOrEmpty(wordResult.Description))            
                return await DownloadWordInfo(wordResult);
            return wordResult;
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
            // Validate Language
            bool isLanguageValid = await ValidateLanguage(language);
            if (!isLanguageValid)                         
                return false;

            // Load Dictionary if not loaded yet
            await LoadDictionary(language);

            var isValid = false;
            lock (dictionaryLock)
            {
                isValid = dictionaries[language].Any(w => w == word.ToUpper());                
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
