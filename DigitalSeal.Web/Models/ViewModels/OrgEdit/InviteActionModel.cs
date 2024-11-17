namespace DigitalSeal.Web.Models.ViewModels.Organization
{
    public class InviteActionModel
    {
        public int OrgID { get; set; }
        public string Code { get; set; } = string.Empty;

        public InviteActionModel(int orgID, string code)
        {
            OrgID = orgID;
            Code = code;
        }

        public InviteActionModel()
        {
        }
    }
}
