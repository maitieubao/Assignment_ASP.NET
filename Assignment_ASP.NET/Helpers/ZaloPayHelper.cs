using System.Security.Cryptography;
using System.Text;

namespace Assignment_ASP.NET.Helpers
{
    public static class ZaloPayHelper
    {
        public static string SignHmacSHA256(string data, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(dataBytes);

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public static bool VerifyHmacSHA256(string data, string signature, string key)
        {
            var expectedSignature = SignHmacSHA256(data, key);
            return expectedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
        }
    }
}
