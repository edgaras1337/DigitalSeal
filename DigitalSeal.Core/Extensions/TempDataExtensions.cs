using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace DigitalSeal.Core.Extensions
{
    public static class TempDataExtensions
    {
        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            tempData[key] = JsonConvert.SerializeObject(value);
        }

        public static T? Get<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            tempData.TryGetValue(key, out object? obj);
            return obj == null ? null : JsonConvert.DeserializeObject<T>((string)obj);
        }

        public static T? Peek<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            if (!tempData.ContainsKey(key))
            {
                return null;
            }
            var obj = tempData.Peek(key);
            return obj == null ? null : JsonConvert.DeserializeObject<T>((string)obj);
        }

        public static bool GetBool(this ITempDataDictionary tempData, string key)
        {
            if (tempData.TryGetValue(key, out object? obj))
            {
                return Convert.ToBoolean(obj);
            }
            return false;
        }
    }
}
