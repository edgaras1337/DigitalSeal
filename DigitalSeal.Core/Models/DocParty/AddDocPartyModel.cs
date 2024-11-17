namespace DigitalSeal.Core.Models.DocParty
{
    public class AddDocPartyModel
    {
        public AddDocPartyModel(int docId, int[] partyIds)
        {
            DocId = docId;
            PartyIds = partyIds;
        }

        public AddDocPartyModel()
        {
        }

        public int DocId { get; set; }
        public int[] PartyIds { get; set; } = [];
    }
}
