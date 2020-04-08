using NBsoft.Wordzz.Core.Models;

namespace NBsoft.Wordzz.Entities
{
    internal class UserSettings : IUserSettings
    {
        public string UserName { get; set; }
        public string MainSettings { get; set; }
        public string WindowsSettings { get; set; }
        public string AndroidSettings { get; set; }
        public string IOSSettings { get; set; }
    }
}
