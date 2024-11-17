using DigitalSeal.Data.Models;

namespace DigitalSeal.Core.Services
{
    public interface INotificationService
    {
        Task<int> GetUnseenCount(int userId);
        Task<List<UserNotification>> GetUserNotificationsAsync(int userId);
        Task MarkSeenAsync(int userId);
        Task MarkSeenAsync(IEnumerable<UserNotification> users);
    }
}