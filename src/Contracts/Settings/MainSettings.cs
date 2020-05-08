using System;

namespace NBsoft.Wordzz.Contracts.Settings
{
    [Serializable]
    public class MainSettings
    {
        public string UserId { get; set; }
        public string Language { get; set; }
        public int DefaultBoard { get; set; }
    }
}
