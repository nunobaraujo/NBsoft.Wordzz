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

        public DicionarioAbertoApi(DictionaryApiSettings settings)
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
            var uri = $"{settings.ApiUrl}/word/{word.ToLower()}";
            var result = await cli.GetAsync(uri);
            var content = await result.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
                return word;

            content = content.Substring(1, content.Length - 2);

            var res = content.FromJson<ApiResult>();
            var shortdescriptions = ExtractFromXml(res.xml);
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
                    var parsed = dNode.InnerXml.Split("\r\n");
                    result.AddRange(parsed.Where(p => !string.IsNullOrEmpty(p)));
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
