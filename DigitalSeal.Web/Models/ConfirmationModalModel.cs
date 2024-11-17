namespace DigitalSeal.Web.Models
{
    public enum ButtonType
    {
        Primary,
        Secondary,
        Danger,
    };

    public class BaseModal
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? MainButtonTitle { get; set; }

        public ButtonType ButtonType { get; set; }
        public string ButtonStateClass => ButtonType switch
        {
            ButtonType.Primary => "primary-button",
            ButtonType.Danger => "danger-button",
            _ => "neutral-button",
        };
    }

    public class GridModal : BaseModal
    {
        public string GridId { get; set; } = string.Empty;
        public string ColumnDefs { get; set; } = string.Empty;
    }

    public class ConfirmationModalModel : BaseModal
    {
        public string Body { get; set; } = string.Empty;
    }
}
