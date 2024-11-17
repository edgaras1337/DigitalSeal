using Microsoft.Extensions.Configuration;

namespace DigitalSeal.Core.Extensions
{
    /// <summary>
    /// Extension class for <see cref="IConfiguration"/>.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Converts the given configuration into a dictionary of keys and values.
        /// </summary>
        /// <param name="config">Configuration object to convert.</param>
        /// <param name="stripSectionPath"></param>
        /// <returns>Dictionary of configuration's keys and values.</returns>
        public static Dictionary<string, string> ToDictionary(this IConfiguration config, bool stripSectionPath = true)
        {
            var data = new Dictionary<string, string>();
            var section = stripSectionPath ? config as IConfigurationSection : null;
            ConvertToDictionary(config, data, section);
            return data;
        }

        private static void ConvertToDictionary(IConfiguration config, Dictionary<string, string>? data = null, IConfigurationSection? top = null)
        {
            data ??= [];
            var children = config.GetChildren();
            foreach (var child in children)
            {
                if (child.Value == null)
                {
                    ConvertToDictionary(config.GetSection(child.Key), data);
                    continue;
                }

                var key = top != null ? child.Path[(top.Path.Length + 1)..] : child.Path;
                data[key] = child.Value;
            }
        }
    }
}
