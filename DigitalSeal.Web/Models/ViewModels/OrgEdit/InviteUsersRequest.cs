namespace DigitalSeal.Web.Models.ViewModels.OrgEdit
{
    public class InviteUsersRequest
    {
        public int OrgId { get; set; }
        public int[] UserIds { get; set; } = [];
    }
}
