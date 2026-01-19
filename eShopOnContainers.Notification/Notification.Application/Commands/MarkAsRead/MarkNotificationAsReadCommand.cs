using MediatR;

namespace Notification.Application.Commands.MarkAsRead
{
    public record MarkNotificationAsReadCommand(Guid NotificationId) : IRequest<bool>;

    public record MarkAllNotificationsAsReadCommand(Guid UserId) : IRequest<bool>;
}
