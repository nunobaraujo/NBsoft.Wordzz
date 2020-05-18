using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Options;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core;
using NBsoft.Wordzz.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.DictionaryApi
{
    public class DictionaryApiFactory
    {
        public static class DictionaryType
        {
            public const string OxfordDictionaries = "OxfordDictionaries";
            public const string DicionarioAberto = "DicionarioAberto";
        }

        private readonly WordzzSettings settings;
        private readonly ILogger log;

        public DictionaryApiFactory(IOptions<WordzzSettings> settings, ILogger log)
        {
            this.settings = settings.Value;
            this.log = log;
        }

        public IDictionaryApi CreateDictionaryApi(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
                throw new ArgumentNullException("Argument cultureInfo cannot be null");
            var language = cultureInfo.Name.ToLower();
            var setting = settings.Dictionaries.SingleOrDefault(d => d.Language.ToLower() == language);
            if (setting == null)
                throw new ArgumentOutOfRangeException($"Invalid culture {cultureInfo.Name}");

            return setting.Type switch
            {
                DictionaryType.OxfordDictionaries => new OxfordDictionariesApi(setting, log),
                DictionaryType.DicionarioAberto => new DicionarioAbertoApi(setting, log),
                _ => throw new InvalidOperationException($"Invalid setting. Dictionaries.Type:{setting.Type}"),
            };
        }
    }
}
