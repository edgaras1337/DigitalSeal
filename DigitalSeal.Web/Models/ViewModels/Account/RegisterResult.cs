using DigitalSeal.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace DigitalSeal.Web.Models.ViewModels.Account
{
    public record RegisterResult(
        IdentityResult IdentityResult, 
        User? User = null);
}
