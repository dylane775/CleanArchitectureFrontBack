using Notification.Domain.Entities;

namespace Notification.Domain.Repositories
{
    public interface INotificationRepository
    {
        Task<UserNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserNotification>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserNotification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserNotification> AddAsync(UserNotification notification, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserNotification notification, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
        Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
