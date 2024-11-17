namespace DigitalSeal.Web.Models.ViewModels.DocEdit
{
    public class UpdateDocumentResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsDeadlinePassed { get; set; }
        public string Deadline { get; set; } = string.Empty;
    }
}