using System.Text;
using System.Security.Cryptography;

namespace DAL.Security
{
    public static class PasswordHasher
    {
        public static (byte[] hash, byte[] salt) HashPassword(string password)
        {
            using var hmac = new HMACSHA256();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return (hash, salt);
        }

        public static bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA256(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(hash);
        }
    }

}
