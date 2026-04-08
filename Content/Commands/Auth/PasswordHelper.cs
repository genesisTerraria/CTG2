// using System;
// using System.Security.Cryptography;
// using System.Text;

// namespace CTG2.Content.Commands.Auth
// {
//     public static class PasswordHelper
//     {
//         public static string HashPassword(string password)
//         {
//             using var sha = SHA256.Create();
//             byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
//             return Convert.ToHexString(bytes).ToLower();
//         }
//     }
// }