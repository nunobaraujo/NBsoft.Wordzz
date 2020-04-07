using System;

namespace NBsoft.Wordzz.Core.Models
{
    interface IUser
    {
        string UserName { get; }
        DateTime CreationDate { get; }
        string PasswordHash { get; }
        string Salt { get; }
        bool Deleted { get; }
    }
}
