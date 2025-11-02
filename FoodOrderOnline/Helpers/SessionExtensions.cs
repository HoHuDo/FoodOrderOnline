using System.Text.Json;

namespace FoodOrderOnline.Helpers
{
    public static class SessionExtensions
    {
        // Hàm để lưu một đối tượng (object) vào Session
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Hàm để đọc một đối tượng (object) từ Session
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonSerializer.Deserialize<T>(value);
        }
    }
}