namespace DigitalSeal.Core.Utilities
{
    public record TimedRequest
    {
        public string? TimeZone { get; set; }
    }

    public enum DateFormat 
    {
        Default,
        DropSeconds 
    };

    /// <summary>
    /// Helper class used for date value formatting.
    /// </summary>
    public class DateHelper
    {
        /// <summary>
        /// Formats date into a readable format, down to seconds.
        /// </summary>
        /// <param name="date">Date to format.</param>
        /// <returns>Formatted date string.</returns>
        public static string FormatDateTime(DateTime date) => date.ToString("yyyy-MM-dd HH:mm:ss");

        //public static string FormatDateTimeInput(DateTime date) => date.ToString("yyyy-MM-ddThh:mm");

        /// <summary>
        /// Formats date into a readable format, without seconds (down to minuted).
        /// </summary>
        /// <param name="date">Date to format.</param>
        /// <returns>Formatted date string.</returns>
        public static string FormatDateTimeDropSeconds(DateTime date) => date.ToString("yyyy-MM-ddTHH:mm");

        public static DateTime ToLocalTime(DateTime dateTime, string? timeZone)
        {
            if (string.IsNullOrEmpty(timeZone) || 
                !TimeZoneInfo.TryFindSystemTimeZoneById(timeZone, out TimeZoneInfo? timeZoneInfo))
            {
                return dateTime;
            }

            return dateTime.Kind == DateTimeKind.Local
                ? TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo)
                : TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneInfo);
        }

        public static string ConvertAndFormat(DateTime dateTime, string? timeZone, DateFormat format = DateFormat.Default)
        {
            DateTime localTime = ToLocalTime(dateTime, timeZone);
            return format switch
            {
                DateFormat.DropSeconds => FormatDateTimeDropSeconds(localTime),
                _ => FormatDateTime(localTime),
            };
        }
    }
}
