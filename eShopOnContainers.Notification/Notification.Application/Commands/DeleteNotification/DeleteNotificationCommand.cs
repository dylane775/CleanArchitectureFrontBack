using MediatR;

namespace Notification.Application.Commands.DeleteNotification
{
    public record DeleteNotificationCommand(Guid NotificationId) : IRequest<bool>;

    public record DeleteAllNotificationsCommand(Guid UserId) : IRequest<bool>;
}
