using DigitalSeal.Core.Attributes;

namespace DigitalSeal.Core.ListProviders.OrgList
{
    public class OrgListModel
    {
        [GridKey]
        public int Id { get; set; }

        [GridColOrder(0)]
        public string Name { get; set; } = string.Empty;

        [GridColumnHidden]
        public bool IsOwner { get; set; }

        [GridColOrder(1)]
        public string Owner { get; set; } = string.Empty;

        [GridColOrder(2)]
        public bool IsCurrent { get; set; }
    }
}
