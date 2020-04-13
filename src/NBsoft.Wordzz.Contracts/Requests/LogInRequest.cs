using System;

namespace NBsoft.Wordzz.Contracts.Requests
{
    [Serializable]
    public class LogInRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
