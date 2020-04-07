using NBsoft.Wordzz.Core.Models;
using System;

namespace NBsoft.Wordzz.Models
{
    class Session : ISession
    {
        public string SessionToken { get; set; }
        public string UserId { get; set; }
        public string UserInfo { get; set; }
        public DateTime Registered { get; set; }
        public DateTime LastAction { get; set; }
        public DateTime Expired { get; set; }
    }
}
