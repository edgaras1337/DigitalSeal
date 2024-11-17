using System.Reflection;
using DigitalSeal.Core.Attributes;

namespace DigitalSeal.Web.Utilities
{
    public class GridResponseHelper
    {
        public static IEnumerable<string> GetModelProperties<TModel>() 
            where TModel : class
        {
            foreach (var property in typeof(TModel).GetProperties())
            {
                if (property.GetCustomAttribute<IgnorePropertyAttribute>() != null)
                    continue;
                else 
                    yield return property.Name;
            }
        }
    }
}
