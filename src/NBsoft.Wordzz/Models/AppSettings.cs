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
        public DbSettings Db { get; set; }
    }
    public class DbSettings
    {
        public string DbType { get; set; }
        public string LogConnString { get; set; }
        public string MainConnString { get; set; }
        public string SessionConnString { get; set; }
    }
}
