using MediatR;
using Notification.Application.DTOs;

namespace Notification.Application.Queries.GetNotifications
{
    public record GetNotificationsQuery(Guid UserId, int Skip = 0, int Take = 20) : IRequest<IEnumerable<NotificationDto>>;

    public record GetUnreadNotificationsQuery(Guid UserId) : IRequest<IEnumerable<NotificationDto>>;

    public record GetNotificationCountQuery(Guid UserId) : IRequest<NotificationCountDto>;
}
