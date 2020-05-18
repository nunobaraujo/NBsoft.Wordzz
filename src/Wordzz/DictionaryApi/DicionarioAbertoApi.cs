
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core;
using NBsoft.Wordzz.Extensions;
using NBsoft.Wordzz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace NBsoft.Wordzz.DictionaryApi
{
    public class DicionarioAbertoApi : IDictionaryApi
    {
        private readonly DictionaryApiSettings settings;
        private readonly ILogger log;

        public DicionarioAbertoApi(DictionaryApiSettings settings, ILogger log)
        {
            this.settings = settings;
            this.log = log;
        }
        public async Task<string> GetDescription(string word)
        {
            return await GetAPIDescription(word);
        }

        private async Task<string> GetAPIDescription(string word)
        {
            var cli = new HttpClient();
            var uri = $"{settings.ApiUrl}/word/{word.ToLower()}";
            var result = await cli.GetAsync(uri);
            var content = await result.Content.ReadAsStringAsync();

#if DEBUG
            log.Debug(content, context: word);
#endif

            if (string.IsNullOrEmpty(content))
                return word;

            var res = content.FromJson<ApiResult[]>();
            var allDescriptions = new List<string>();
            foreach (var item in res)
            {
                allDescriptions.AddRange(ExtractFromXml(item.xml));
            }

            var shortdescriptions = allDescriptions.ToArray();
            if (shortdescriptions.Length > 4)
            {
                var newArray = new string[4];
                Array.Copy(shortdescriptions, newArray, 4);
                shortdescriptions = newArray;
            }
            return string.Join(";", shortdescriptions);
        }

        private string[] ExtractFromXml(string xml)
        {
            var rdr = new XmlDocument();
            rdr.LoadXml(xml);
            var defs = rdr.GetElementsByTagName("def");
            var result = new List<string>();
            foreach (var def in defs)
            {
                var dNode = def as XmlNode;
                if (!string.IsNullOrEmpty(dNode.InnerXml))
                {
                    var parsed = dNode.InnerXml.Split("\n")
                        .Select(x => x.Replace("\r","")
                        .Replace("[[","(")
                        .Replace("]]", ")"));

                    var notEmpty = parsed.Where(p => !string.IsNullOrEmpty(p));
                    result.AddRange(notEmpty);
                }
            }
            return result.ToArray();
        }
        private class ApiResult
        {
            public string word { get; set; }
            public int word_id { get; set; }
            public string xml { get; set; }
        }
    }
}
