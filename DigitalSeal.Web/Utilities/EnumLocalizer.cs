using Microsoft.Extensions.Localization;

namespace DigitalSeal.Web.Utilities
{
    public class EnumLocalizer
    {
        public static IDictionary<string, string> Localize<TEnum>(IStringLocalizer loc)
            where TEnum : struct, Enum
        {
            Dictionary<string, string> categories = Enum.GetValues<TEnum>()
                .Cast<TEnum>()
                .ToDictionary(x => x.ToString(), x => loc[x.ToString()].Value);

            return categories;
        }
    }
}
