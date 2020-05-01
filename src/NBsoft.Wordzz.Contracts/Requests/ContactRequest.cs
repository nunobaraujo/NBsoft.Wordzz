using System;

namespace NBsoft.Wordzz.Contracts.Requests
{
    [Serializable]
    public class ContactRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
