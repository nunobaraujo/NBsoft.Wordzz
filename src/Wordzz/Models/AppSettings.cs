namespace NBsoft.Wordzz.Models
{
    public class AppSettings
    {
        public WordzzSettings Wordzz { get; set; }
    }
    public class WordzzSettings
    {
        public string ApiKey { get; set; }
        public string EncryptionKey { get; set; }
        public string ServerId { get; set; }
        public bool UseLexiconCache { get; set; }
        public DictionaryApiSettings[] Dictionaries { get; set; }
        public DbSettings Db { get; set; }
    }
    public class DbSettings
    {
        public string DbType { get; set; }
        public string LogConnString { get; set; }
        public string MainConnString { get; set; }
        public string SessionConnString { get; set; }
        public string StatsConnString { get; set; }
    }
    public class DictionaryApiSettings
    {
        public string Type { get; set; }
        public string Language { get; set; }
        public string ApiUrl { get; set; }
        public string ApiAppId { get; set; }
        public string ApiKey { get; set; }
    }
}
