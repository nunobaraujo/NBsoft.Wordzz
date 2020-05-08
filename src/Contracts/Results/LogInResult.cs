using System;

namespace NBsoft.Wordzz.Contracts.Results
{
    [Serializable]
    public class LogInResult
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Token { get; set; }
    }
}
