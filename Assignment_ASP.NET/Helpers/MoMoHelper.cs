using System.Security.Cryptography;
using System.Text;

namespace Assignment_ASP.NET.Helpers
{
    public static class MoMoHelper
    {
        public static string SignHmacSHA256(string message, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(messageBytes);
            
            var hex = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }
    }
}
