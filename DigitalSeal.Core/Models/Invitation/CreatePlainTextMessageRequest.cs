namespace DigitalSeal.Core.Models.Invitation
{
    public record CreatePlainTextMessageRequest(int SenderId, int ReceiverId, string Title, string Content);
}
