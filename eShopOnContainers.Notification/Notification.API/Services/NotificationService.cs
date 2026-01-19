using Microsoft.AspNetCore.SignalR;
using Notification.API.Hubs;
using Notification.Application.DTOs;

namespace Notification.API.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(NotificationDto notification);
        Task SendNotificationToUserAsync(Guid userId, NotificationDto notification);
        Task SendUnreadCountUpdateAsync(Guid userId, int count);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendNotificationAsync(NotificationDto notification)
        {
            await SendNotificationToUserAsync(notification.UserId, notification);
        }

        public async Task SendNotificationToUserAsync(Guid userId, NotificationDto notification)
        {
            try
            {
                await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
                _logger.LogInformation("Notification sent to user {UserId}: {Title}", userId, notification.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
            }
        }

        public async Task SendUnreadCountUpdateAsync(Guid userId, int count)
        {
            try
            {
                await _hubContext.Clients.Group($"user_{userId}").SendAsync("UnreadCountUpdated", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send unread count update to user {UserId}", userId);
            }
        }
    }
}
