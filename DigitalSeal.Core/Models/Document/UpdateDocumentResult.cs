namespace DigitalSeal.Core.Models.Document
{
    public class UpdateDocumentResult
    {
        public string Status { get; set; } = string.Empty;
        public bool IsDeadlinePassed { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
    }
}
