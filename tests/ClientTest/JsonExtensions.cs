using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ClientTest
{

    public static class JsonExtensions
    {
        public static string ToJson(this object value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }
        public static T FromJson<T>(this string jsonString)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString);
        }

        //public static IEnumerable<string> GetAllValues(this string jsonString, string key)
        //{
        //    JObject jObject = JObject.Parse(jsonString);

        //    // .. - recursive descent
        //    var classNameTokens = jObject.SelectTokens($"..{key}");
        //    var values = classNameTokens.Select(x =>
        //    {
        //        return (x as JArray).First?.ToString();
        //    });
        //    return values.ToArray();
        //}
    }

}
