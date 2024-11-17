namespace DigitalSeal.Web.Models.ViewModels.DocEdit
{
    public class GetDocumentRequest
    {
        public int DocId { get; set; }

        public GetDocumentRequest()
        {
        }

        public GetDocumentRequest(int docId)
        {
            DocId = docId;
        }
    }
}
