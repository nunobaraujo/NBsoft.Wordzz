using NBsoft.Wordzz.Core.Models;
using System;

namespace NBsoft.Wordzz.Entities
{
    internal class User : IUser
    {
        public string UserName { get; set; }
        public DateTime CreationDate { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public bool Deleted { get; set; }
    }
}
