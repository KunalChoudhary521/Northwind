using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Northwind.API.Services
{
    public class CryptoService : ICryptoService
    {
        public void EncryptPassword(string password, out byte[] salt, out byte[] hash)
        {
            salt = null;
            hash = null;
            if (string.IsNullOrWhiteSpace(password))
                return;

            using var hmac = new HMACSHA256();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public bool IsPasswordCorrect(string password, byte[] testSalt, byte[] testHash)
        {
            if (string.IsNullOrWhiteSpace(password) || testSalt.Length != 64 || testHash.Length != 32)
                return false;

            using var hmac = new HMACSHA256(testSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return !computedHash.Where((t, i) => t != testHash[i]).Any();
        }
    }
}