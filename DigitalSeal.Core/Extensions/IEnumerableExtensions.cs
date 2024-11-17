namespace DigitalSeal.Core.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string ToReadableString<T>(this IEnumerable<T> lst)
        {
            if (lst.Count() == 1)
            {
                return lst.First()?.ToString() ?? string.Empty;
            }

            return string.Join(", ", lst);
        }
    }
}
