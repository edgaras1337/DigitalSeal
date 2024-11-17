namespace DigitalSeal.Web.Models.ViewModels.Account
{
    public record EmailConfirmationModel(
        string Email, EmailConfirmationType 
        Type = EmailConfirmationType.Registration);
}
