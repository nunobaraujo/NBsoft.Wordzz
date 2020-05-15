using Microsoft.Extensions.Options;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
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
        private readonly WordzzSettings settings;
        private readonly Dictionary<string, IEnumerable<string>> dictionaries;        
        
        private IEnumerable<ILexicon> availableDictionaries;
        
        


        public LexiconService(ILogger logger, IWordRepository wordRepository, IOptions<WordzzSettings> settings)
        {
            this.logger = logger;
            this.wordRepository = wordRepository;
            this.settings = settings.Value;
            dictionaries = new Dictionary<string, IEnumerable<string>>();
            availableDictionaries = new List<ILexicon>();

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
        public async Task<IWord> GetWordInfo(string language, string word)
        {
            bool isLanguageValid = await ValidateLanguage(language);
            if (!isLanguageValid)
                return null;

            var lexicon = availableDictionaries.Single(l => l.Language == language);
            var wordResult = await wordRepository.Get(lexicon, word);
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
                newDescritiption = await GetAPIDescription(culture.Name, word.Name);
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
        private async Task<string> GetAPIDescription(string language, string word)
        {   
            var cli = new HttpClient();
            var uri = $"{settings.DictionaryApiUrl}/entries/{language.ToLower()}/{word.ToLower()}";
            cli.DefaultRequestHeaders.Add("app_id", settings.DictionaryApiAppId);
            cli.DefaultRequestHeaders.Add("app_key", settings.DictionaryApiKey);
            var result = await cli.GetAsync(uri);
            var retval = await result.Content.ReadAsStringAsync();
                        
            if (retval.Contains("error"))
            {
                uri = $"{settings.DictionaryApiUrl}/lemmas/{language.ToLower()}/{word.ToLower()}";
                result = await cli.GetAsync(uri);
                retval = await result.Content.ReadAsStringAsync();
                var reference = retval.GetAllValues("inflectionOf").FirstOrDefault();
                var inflection = reference.FromJson<Inflection>();
                if (inflection != null)
                {
                    uri = $"{settings.DictionaryApiUrl}/entries/{language.ToLower()}/{inflection.Id}";
                    result = await cli.GetAsync(uri);
                    retval = await result.Content.ReadAsStringAsync();                    
                }
            }
            
            var shortdescriptions = retval.GetAllValues("shortDefinitions").ToArray();
            if (shortdescriptions.Length>4)
            {
                var newArray = new string[4];
                Array.Copy(shortdescriptions, newArray, 4);
                shortdescriptions = newArray;
            }
            return string.Join(";", shortdescriptions);
        }

        public async Task<IEnumerable<ILexicon>> AvailableLexicons()
        {
            return await wordRepository.ListDictionaries();
        }
    }
}
