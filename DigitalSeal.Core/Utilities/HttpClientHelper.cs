using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace DigitalSeal.Core.Utilities
{
    /// <summary>
    /// Helper class for managing various HTTP functions.
    /// </summary>
    public class HttpClientHelper
    {
        public static async Task<TResponse?> CallAsync<TRequest, TResponse>(string httpMethod, string address, TRequest? requestData = default,
            string mimeType = MimeTypes.Xml)
        {
            using var client = new HttpClient();

            var response = httpMethod switch
            {
                WebRequestMethods.Http.Get => await client.GetAsync(address),
                WebRequestMethods.Http.Post => await CreatePostAsync(client, address, requestData, mimeType),
                _ => null,
            };
            if (response == null)
            {
                return default;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public static async Task<HttpResponseMessage?> CallAsync<TRequest>(string httpMethod, string address, TRequest? requestData = default,
            string mimeType = MimeTypes.Xml)
        {
            using var client = new HttpClient();

            var response = httpMethod switch
            {
                WebRequestMethods.Http.Get => await client.GetAsync(address),
                WebRequestMethods.Http.Post => await CreatePostAsync(client, address, requestData, mimeType),
                _ => null,
            };
            if (response == null)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return response;
        }

        private static async Task<HttpResponseMessage> CreatePostAsync<TRequest>(HttpClient client, string url, TRequest requestData, string mimeType)
        {
            StringContent content;
            if (mimeType == MimeTypes.Json)
            {
                content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, MimeTypes.Json);
            }
            else if (mimeType == MimeTypes.Xml)
            {
                content = new StringContent(ConvertToXml(requestData!), Encoding.UTF8, MimeTypes.Xml);
            }
            else
            {
                throw new ArgumentException("Invalid mime type.", nameof(mimeType));
            }

            return await client.PostAsync(url, content);
        }

        private static string ConvertToXml(object obj)
        {
            var xmlWriterSettings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true
            };
            using var sww = new Utf8StringWriter();
            using XmlWriter writer = XmlWriter.Create(sww, xmlWriterSettings);
            var serializer = new XmlSerializer(obj.GetType());
            serializer.Serialize(writer, obj);
            string xml = sww.ToString();
            return xml;
        }

        private sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }

        /// <summary>
        /// Commonly used MIME types.
        /// </summary>
        public class MimeTypes
        {
            public const string Json = "application/json";
            public const string Xml = "application/xml";
            public const string Pdf = "application/pdf";
        }
    }
}
