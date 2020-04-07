using System;

namespace NBsoft.Wordzz.Core.Models
{
    public interface ISession
    {
        string SessionToken { get; }
        string UserId { get; }
        string UserInfo { get; }
        DateTime Registered { get; }
        DateTime LastAction { get; }
        DateTime Expired { get; }
    }
}
