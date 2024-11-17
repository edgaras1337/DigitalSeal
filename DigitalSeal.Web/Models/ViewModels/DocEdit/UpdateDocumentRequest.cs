namespace DigitalSeal.Web.Models.ViewModels.DocEdit
{
    public class UpdateDocumentRequest
    {
        public int DocId { get; set; }

        //[Required(ErrorMessage = "General.Required.DocName")]
        public string? Name { get; set; }// = string.Empty;

        //[Required(ErrorMessage = "General.Required.DocDeadline")]
        public string? Deadline { get; set; }// = string.Empty;
    }
}
