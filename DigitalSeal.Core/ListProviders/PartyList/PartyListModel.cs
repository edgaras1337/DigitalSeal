using DigitalSeal.Core.Attributes;

namespace DigitalSeal.Core.ListProviders.PartyList
{
    public class PartyListModel
    {
        [GridKey, GridColumnHidden]
        public int Id { get; set; }

        [GridColOrder(0)]
        public string Email { get; set; } = string.Empty;

        [GridColOrder(1)]
        public string FirstName { get; set; } = string.Empty;

        [GridColOrder(2)]
        public string LastName { get; set; } = string.Empty;
    }
}
