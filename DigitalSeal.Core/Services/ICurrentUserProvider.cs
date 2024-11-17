using DigitalSeal.Data.Models;

namespace DigitalSeal.Core.Services
{
    public interface ICurrentUserProvider
    {
        int CurrentUserId { get; }
        Task<Party> GetCurrentPartyAsync();
        Task<int> GetCurrentPartyIdAsync();
        Task<User> GetCurrentUserAsync();
    }
}