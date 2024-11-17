namespace DigitalSeal.Web.Models.ViewModels.OrgEdit
{
    public class RespondToOrgInviteRequest
    {
        public int InviteId { get; set; }
        public bool IsAccept { get; set; }
        public string ReturnUrl { get; set; } = "/";
    }
}
