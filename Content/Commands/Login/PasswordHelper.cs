using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CTG2.Content.Commands
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        public static string GetStoredHash(string path)
        {
            return File.Exists(path) ? File.ReadAllText(path).Trim() : null;
        }
    }
}
