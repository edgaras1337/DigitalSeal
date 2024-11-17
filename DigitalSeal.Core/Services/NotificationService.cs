using DigitalSeal.Data.Models;
using DigitalSeal.Data.Repositories;

namespace DigitalSeal.Core.Services
{
    internal class NotificationService : INotificationService
    {
        private readonly IUserNotificationRepository _userNotificationRepository;
        public NotificationService(IUserNotificationRepository userNotificationRepository)
        {
            _userNotificationRepository = userNotificationRepository;
        }

        public Task<List<UserNotification>> GetUserNotificationsAsync(int userId)
            => _userNotificationRepository.GetByUserIdAsync(userId);

        public async Task<int> GetUnseenCount(int userId)
            => await _userNotificationRepository.GetUnseenCountForUserAsync(userId);

        public async Task MarkSeenAsync(int userId)
        {
            List<UserNotification> notifications = await _userNotificationRepository.GetUnseenForUserAsync(userId);
            await MarkSeenAsync(notifications);
        }

        public async Task MarkSeenAsync(IEnumerable<UserNotification> notifications)
        {
            foreach (UserNotification notif in notifications)
                notif.IsSeen = true;

            await _userNotificationRepository.SaveChangesAsync();
        }
    }
}
