using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Encryption;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Entities;
using System;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Extensions
{
    public static class UserRepositoryExtensions
    {
        private const string IV = "017F429CE3924CC6";
        private const string DataProtectionSalt = "3726B4D0-83D1-43AE-873C-8061BC0D5E28";


        internal static IUser SetPassword(this IUser src, string password, string encryptionKey)
        {
            var editable = src.ToDto<User>();
            editable.Salt = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            editable.PasswordHash = Encrypt(password, encryptionKey, editable.Salt);
            return editable;
        }

        internal static bool CheckPassword(this IUser src, string password, string encryptionKey)
        {
            var hash = Encrypt(password, encryptionKey, src.Salt);
            return src.PasswordHash == hash;
        }

        internal static IUserDetails Decrypt(this IUserDetails src, string encryptionKey)
        {
            var decrypted = new UserDetails
            {
                UserName = src.UserName,
                Address = DecryptField(src.Address, encryptionKey),
                City = DecryptField(src.City, encryptionKey),
                Country = DecryptField(src.Country, encryptionKey),
                Email = DecryptField(src.Email, encryptionKey),
                FirstName = DecryptField(src.FirstName, encryptionKey),
                LastName = DecryptField(src.LastName, encryptionKey),
                PostalCode = DecryptField(src.PostalCode, encryptionKey)                
            };
            return decrypted;
        }
        internal static IUserDetails Encrypt(this IUserDetails src, string encryptionKey)
        {
            var decrypted = new UserDetails
            {
                UserName = src.UserName,
                Address = EncryptField(src.Address, encryptionKey),
                City = EncryptField(src.City, encryptionKey),
                Country = EncryptField(src.Country, encryptionKey),
                Email = EncryptField(src.Email, encryptionKey),
                FirstName = EncryptField(src.FirstName, encryptionKey),
                LastName = EncryptField(src.LastName, encryptionKey),
                PostalCode = EncryptField(src.PostalCode, encryptionKey)
            };
            return decrypted;
        }

        internal static async Task<IUser> FindUser(this IUserRepository src, string userNameOrEmail)
        {
            var user = await src.Get(userNameOrEmail);
            if (user == null)
                user = await src.GetByEmail(userNameOrEmail);
            return user;
        }

        private static string Encrypt(string plainText, string encryptionKey, string salt)
        {
            if (string.IsNullOrEmpty(plainText))
                return null;
            if (string.IsNullOrEmpty(encryptionKey))
                throw new ArgumentNullException(nameof(encryptionKey));
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentNullException(nameof(salt));

            return RijndaelSimple.SHA1Encrypt(plainText, encryptionKey, salt, IV);
        }
        private static string Decrypt(string encryptedText, string encryptionKey, string salt)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return null;
            if (string.IsNullOrEmpty(encryptionKey))
                throw new ArgumentNullException(nameof(encryptionKey));
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentNullException(nameof(salt));

            return RijndaelSimple.SHA1Decrypt(encryptedText, encryptionKey, salt, IV);
        }

        private static string DecryptField(this string src, string encryptionKey) => string.IsNullOrEmpty(src) ? "" : Decrypt(src, encryptionKey, DataProtectionSalt);
        public static string EncryptField(this string src, string encryptionKey) => string.IsNullOrEmpty(src) ? "" : Encrypt(src, encryptionKey, DataProtectionSalt);

    }
}
