namespace NBsoft.Wordzz.Extensions
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
    }
}
