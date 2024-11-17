namespace DigitalSeal.Core.Models.Settings
{
    public class UpdateUserResult
    {
        public UpdateUserResult(bool needSignOut, bool is2FAEnabled, string email, bool emailUpdated)
        {
            NeedSignOut = needSignOut;
            Is2FAEnabled = is2FAEnabled;
            Email = email;
            EmailUpdated = emailUpdated;
        }

        public bool NeedSignOut { get; set; }
        public bool Is2FAEnabled { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool EmailUpdated { get; set; }
        public string? EmailConfirmationUrl { get; set; }
    }
}
