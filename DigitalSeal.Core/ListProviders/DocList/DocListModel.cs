using DigitalSeal.Core.Attributes;

namespace DigitalSeal.Core.ListProviders.DocList
{
    public class DocListModel
    {
        [GridKey]
        public int Id { get; set; }

        [GridColOrder(0)]
        public string Name { get; set; } = string.Empty;

        [GridColOrder(1)]
        public string Author { get; set; } = string.Empty;

        [GridColOrder(2)]
        public string CreatedDate { get; set; } = string.Empty;

        // TODO: SHWO IF ITS SIGNED

        [GridColOrder(3)]
        public bool IsSigned { get; set; }
    }
}
