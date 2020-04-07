using NBsoft.Wordzz.Core.Encryption;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Extensions
{
    public static class UserRepositoryExtensions
    {
        private const string IV = "DDC7CAFE60FD4336";
        private const string DataProtectionSalt = "A90637E0-2359-4316-93D5-2230E58D8AA2";


        internal static IUser SetPassword(this IUser src, string password, string encryptionKey)
        {
            var editable = src.ToDto();
            editable.Salt = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            editable.PasswordHash = Encrypt(password, encryptionKey, src.Salt);
            return src;
        }

        internal static bool CheckPassword(this IUser src, string password, string encryptionKey)
        {
            var hash = Encrypt(password, encryptionKey, src.Salt);
            return src.PasswordHash == hash;
        }


        private static string Encrypt(string plainText, string encryptionKey, string salt)
        {
            if (string.IsNullOrEmpty(plainText))
                return null;
            if (string.IsNullOrEmpty(encryptionKey))
                throw new ArgumentNullException(nameof(encryptionKey));
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentNullException(nameof(salt));

            return RijndaelSimple.SHA1Encrypt(plainText, encryptionKey, salt, IV,);
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
    }
}
