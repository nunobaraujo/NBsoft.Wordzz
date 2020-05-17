using NBsoft.Wordzz.Core;
using NBsoft.Wordzz.Extensions;
using NBsoft.Wordzz.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.DictionaryApi
{
    public class OxfordDictionariesApi : IDictionaryApi
    {
        private readonly DictionaryApiSettings settings;

        public OxfordDictionariesApi(DictionaryApiSettings settings)
        {
            this.settings = settings;
        }

        public async Task<string> GetDescription(string word)
        {
            return await GetAPIDescription(word);
        }
        private async Task<string> GetAPIDescription(string word)
        {
            var cli = new HttpClient();
            var uri = $"{settings.ApiUrl}/entries/{settings.Language.ToLower()}/{word.ToLower()}";
            cli.DefaultRequestHeaders.Add("app_id", settings.ApiAppId);
            cli.DefaultRequestHeaders.Add("app_key", settings.ApiKey);
            var result = await cli.GetAsync(uri);
            var retval = await result.Content.ReadAsStringAsync();

            if (retval.Contains("error"))
                retval = await GetFromInflexions(cli, word);

            if (retval == null)
                return word;

            var shortdescriptions = retval.GetAllValues("shortDefinitions").ToArray();
            if (shortdescriptions.Length > 4)
            {
                var newArray = new string[4];
                Array.Copy(shortdescriptions, newArray, 4);
                shortdescriptions = newArray;
            }
            return string.Join(";", shortdescriptions);
        }

        private async Task<string> GetFromInflexions(HttpClient client, string word)
        {
            var uri = $"{settings.ApiUrl}/lemmas/{settings.Language.ToLower()}/{word.ToLower()}";
            var result = await client.GetAsync(uri);
            var lemmas = await result.Content.ReadAsStringAsync();
            if (lemmas.Contains("error"))
                return null;

            var reference = lemmas.GetAllValues("inflectionOf").FirstOrDefault();
            var inflection = reference.FromJson<Inflection>();
            if (inflection != null)
            {
                uri = $"{settings.ApiUrl}/entries/{settings.Language.ToLower()}/{inflection.Id}";
                result = await client.GetAsync(uri);
                return await result.Content.ReadAsStringAsync();
            }
            return null;
    
        }
        private class Inflection
        {
            public string Id { get; set; }
            public string Text { get; set; }
        }
    }

    
}
