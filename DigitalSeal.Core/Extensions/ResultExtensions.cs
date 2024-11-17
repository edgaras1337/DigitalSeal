using LanguageExt.Common;

namespace DigitalSeal.Core.Extensions
{
    public static class ResultExtensions
    {
        public static void Match<T>(this Result<T> result, Action<T> success, Action<Exception> fail)
        {
            _ = result.Match(s =>
            {
                success(s);
                return true;
            }, f =>
            {
                fail(f);
                return true;
            });
        }
    }
}
