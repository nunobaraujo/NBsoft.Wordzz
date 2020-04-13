using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Entities;
using System;
using System.Reflection;

namespace NBsoft.Wordzz.Extensions
{
    internal static class ContractExtensions
    {
        //public static Session ToDto(this ISession src)
        //{
        //    return new Session
        //    {
        //        Expired = src.Expired,
        //        LastAction = src.LastAction,
        //        Registered = src.Registered,
        //        SessionToken = src.SessionToken,
        //        UserId = src.UserId,
        //        UserInfo = src.UserInfo
        //    };
        //}
        //public static User ToDto(this IUser src) 
        //{
        //    return new User
        //    {
        //        CreationDate = src.CreationDate,
        //        Deleted = src.Deleted,
        //        PasswordHash = src.PasswordHash,
        //        Salt = src.Salt,
        //        UserName = src.UserName
        //    };
        //}
        //public static UserDetails ToDto(this IUserDetails src)
        //{
        //    return new UserDetails
        //    {
        //        Address= src.Address,
        //        City = src.City,
        //        Country= src.Country,
        //        Email = src.Email,
        //        FirstName = src.FirstName,
        //        LastName = src.LastName,
        //        PostalCode = src.PostalCode,
        //        UserName = src.UserName
        //    };
        //}

        //public static Board ToDto(this IBoard src)
        //{
        //    return new Board
        //    {
        //        Columns = src.Columns,
        //        Rows = src.Rows,
        //        Tiles = src.Tiles,
        //        Words = src.Words
        //    };
        //}
        public static T ToDto<T>(this object src) where T : new()
        {
            var res = new T();
            foreach (var prop in src.GetType().GetProperties())
            {
                var value = prop.GetValue(src);
                prop.SetValue(res, value, null);
                
            }

            return res;
        }
    }
}
