using NBsoft.Wordzz.Core.Models;
using System;

namespace NBsoft.Wordzz.Models
{
    class UserSession : IUserSession
    {
        public string UserId { get; set; }
        public string SessionToken { get; set; }
        public string UserInfo { get; set; }
        public string ActiveCompany { get; set; }
        public DateTime Registered { get; set; }
        public DateTime LastAction { get; set; }
    }
}
