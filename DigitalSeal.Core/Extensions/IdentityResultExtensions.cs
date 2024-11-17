using DigitalSeal.Core.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace DigitalSeal.Core.Extensions
{
    public static class IdentityResultExtensions
    {
        public static ValidationException ToValidationException(this IdentityResult result, string mainMessage)
        {
            if (result.Succeeded)
                throw new ArgumentException("Result must be failed");

            var details = new List<string>(result.Errors.Count());
            foreach (var error in result.Errors)
            {
                details.Add(error.Description);
            }

            return ValidationException.Error(mainMessage, details);
        }
    }
}
