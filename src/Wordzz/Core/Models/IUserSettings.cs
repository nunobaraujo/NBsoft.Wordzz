namespace NBsoft.Wordzz.Core.Models
{
    public interface IUserSettings
    {
        string UserName { get; }
        string MainSettings { get; }
        string WindowsSettings { get; }
        string AndroidSettings { get; }
        string IOSSettings { get; }
    }
}
