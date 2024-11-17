using DigitalSeal.Data.Models;

namespace DigitalSeal.Core.Utilities
{
    /// <summary>
    /// Helper class for <see cref="User"/>.
    /// </summary>
    public class UserHelper
    {
        /// <summary>
        /// Formats the user id, first name, last name into a readable string.
        /// </summary>
        /// <param name="user">User to format.</param>
        /// <returns>Formatted string of user's id, name, surname.</returns>
        public static string FormatUserName(User? user)
            => user == null ? "User not found" : $"#{user.Id} {user.FirstName} {user.LastName}";

        public static string FormatUserName(int id, string firstName, string lastName)
            => $"#{id} {firstName} {lastName}";
    }
}
