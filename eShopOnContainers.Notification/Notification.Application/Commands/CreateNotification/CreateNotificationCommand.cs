using MediatR;
using Notification.Application.DTOs;

namespace Notification.Application.Commands.CreateNotification
{
    public record CreateNotificationCommand(
        Guid UserId,
        string Type,
        string Title,
        string Message,
        string? ImageUrl = null,
        string? ActionUrl = null,
        string? RelatedEntityId = null,
        Dictionary<string, string>? Metadata = null
    ) : IRequest<NotificationDto>;
}
