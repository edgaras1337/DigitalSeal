using DigitalSeal.Data.Models;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace DigitalSeal.Data.Repositories
{
    internal class UserNotificationRepository : Repository<UserNotification>, IUserNotificationRepository
    {
        public UserNotificationRepository(AppDbContext context) 
            : base(context)
        {
        }

        public async Task<List<UserNotification>> GetByUserIdAsync(int userId)
        {
            return await Context.UserNotifications
                .Include(notif => notif.Sender)
                .Include(notif => notif.PlainTextNotification)
                .Include(notif => notif.InviteNotification)
                .Where(notif => notif.ReceiverId == userId)
                .OrderByDescending(notif => notif.Created)
                .ToListAsync();
        }

        public async Task<int> GetUnseenCountForUserAsync(int userId)
        {
            return await Context.UserNotifications
                .AsNoTracking()
                .Where(notif => notif.ReceiverId == userId && !notif.IsSeen)
                .CountAsync();
        }

        public async Task<List<UserNotification>> GetUnseenForUserAsync(int userId)
        {
            return await Context.UserNotifications
                .Where(notif => notif.ReceiverId == userId && !notif.IsSeen)
                .ToListAsync();
        }

        public async Task<UserNotification?> GetByInviteIdAsync(int inviteId)
        {
            InviteNotification? invite = await Context.InviteNotifications
                .Include(x => x.UserNotification)
                .FirstOrDefaultAsync(x => x.UserNotificationId == inviteId);

            return invite?.UserNotification;
        }
    }

    public interface IUserNotificationRepository : IRepository<UserNotification> 
    {
        Task<List<UserNotification>> GetByUserIdAsync(int userId);
        Task<int> GetUnseenCountForUserAsync(int userId);
        Task<List<UserNotification>> GetUnseenForUserAsync(int userId);
        Task<UserNotification?> GetByInviteIdAsync(int inviteId);
    }
}
