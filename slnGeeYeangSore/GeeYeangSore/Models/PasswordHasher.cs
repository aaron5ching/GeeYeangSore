using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace GeeYeangSore.Models
{
    public static class PasswordHasher
    {
        // 生成鹽值
        public static string GenerateSalt()
        {
            byte[] salt = new byte[128 / 8]; // 128 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        // 使用鹽值加密密碼
        public static string HashPassword(string password, string salt)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            if (string.IsNullOrEmpty(salt))
                throw new ArgumentNullException(nameof(salt));

            byte[] saltBytes = Convert.FromBase64String(salt);
            
            // 使用 PBKDF2 算法進行哈希
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashed;
        }

        // 驗證密碼是否正確
        public static bool VerifyPassword(string password, string salt, string hashedPassword)
        {
            string computedHash = HashPassword(password, salt);
            return computedHash == hashedPassword;
        }
    }
} 