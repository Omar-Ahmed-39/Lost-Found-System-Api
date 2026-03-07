using LostAndFound.Core.Entities;

namespace LostAndFound.Core.Interfaces;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId);
    Task<bool> MarkAsReadAsync(int notificationId, int userId);
}
