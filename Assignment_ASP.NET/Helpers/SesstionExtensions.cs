using System.Text.Json; // Cần thư viện này

namespace Assignment_ASP.NET.Helpers
{
    public static class SessionExtensions
    {
        // Phương thức Set: Lưu đối tượng vào Session
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Phương thức Get: Lấy đối tượng từ Session
        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}