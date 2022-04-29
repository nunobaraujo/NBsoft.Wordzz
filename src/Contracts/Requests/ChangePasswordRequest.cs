using System;

namespace NBsoft.Wordzz.Contracts.Requests
{
    [Serializable]
    public class ChangePasswordRequest
    {
        public string Email { get; set; }
    }
}
