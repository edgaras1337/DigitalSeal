using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DigitalSeal.Core.Utilities
{
    public class JsonHelper
    {
        public static JsonSerializerSettings CamelCase() => new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        public static string ToJsonCamelCase<T>(T data)
            => JsonConvert.SerializeObject(data, CamelCase());
    }
}
