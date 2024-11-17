namespace DigitalSeal.Web.Models.ViewModels.OrgEdit
{
    public class OrgEditViewModel : BasePageModel
    {
        public int OrgId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public bool IsOwner { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string CreatedDate { get; set; } = string.Empty;
        public bool IsCurrent { get; set; }
        public string PartyColumnDefs { get; set; } = string.Empty;
        public string PossiblePartyColumnDefs { get; set; } = string.Empty;
        public string PendingPartyColumnDefs { get; set; } = string.Empty;
    }
}
