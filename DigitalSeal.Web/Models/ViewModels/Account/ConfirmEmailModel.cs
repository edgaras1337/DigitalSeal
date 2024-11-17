namespace DigitalSeal.Web.Models.ViewModels.Account
{
    public enum EmailConfirmationType { Registration, ChangeEmail };
    public record ConfirmEmailModel(
        string Email, 
        string Token, 
        EmailConfirmationType Type = EmailConfirmationType.Registration);
}
