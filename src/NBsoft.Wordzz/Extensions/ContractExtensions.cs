using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Entities;

namespace NBsoft.Wordzz.Extensions
{
    internal static class ContractExtensions
    {
        public static Session ToDto(this ISession src)
        {
            return new Session
            {
                Expired = src.Expired,
                LastAction = src.LastAction,
                Registered = src.Registered,
                SessionToken = src.SessionToken,
                UserId = src.UserId,
                UserInfo = src.UserInfo
            };
        }
        public static User ToDto(this IUser src) 
        {
            return new User
            {
                CreationDate = src.CreationDate,
                Deleted = src.Deleted,
                PasswordHash = src.PasswordHash,
                Salt = src.Salt,
                UserName = src.UserName
            };
        }
    }
}
