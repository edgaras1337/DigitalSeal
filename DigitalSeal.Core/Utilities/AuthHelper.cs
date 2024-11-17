using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Cryptography;

namespace DigitalSeal.Core.Utilities
{
    public class AuthHelper
    {
        public static int? GetCurrentUserID(HttpContext context)
        {
            var user = context.User;
            if (user == null)
            {
                return null;
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userId) ? null : Convert.ToInt32(userId);
        }

        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;

            using (var bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }

            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
    }
}
