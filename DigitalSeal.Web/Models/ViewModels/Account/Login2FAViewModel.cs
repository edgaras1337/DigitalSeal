namespace DigitalSeal.Web.Models.ViewModels.Account
{
    public class Login2FAViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
    }
}
