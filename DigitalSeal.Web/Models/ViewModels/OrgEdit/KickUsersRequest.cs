namespace DigitalSeal.Web.Models.ViewModels.OrgEdit
{
    public class KickUsersRequest
    {
        public int OrgId { get; set; }
        public int[] PartyIds { get; set; } = [];
    }
}
