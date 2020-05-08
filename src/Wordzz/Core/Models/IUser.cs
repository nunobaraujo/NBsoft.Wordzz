using System;

namespace NBsoft.Wordzz.Core.Models
{
    public interface IUser
    {
        string UserName { get; }
        DateTime CreationDate { get; }
        string PasswordHash { get; }
        string Salt { get; }
        bool Deleted { get; }
    }
}
