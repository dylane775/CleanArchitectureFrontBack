using MediatR;
using Notification.Application.DTOs;
using Notification.Domain.Repositories;

namespace Notification.Application.Queries.GetNotifications
{
    public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, IEnumerable<NotificationDto>>
    {
        private readonly INotificationRepository _repository;

        public GetNotificationsQueryHandler(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            var notifications = await _repository.GetByUserIdAsync(request.UserId, request.Skip, request.Take, cancellationToken);

            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                ImageUrl = n.ImageUrl,
                ActionUrl = n.ActionUrl,
                RelatedEntityId = n.RelatedEntityId,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt ?? DateTime.UtcNow,
                Metadata = n.Metadata
            });
        }
    }

    public class GetUnreadNotificationsQueryHandler : IRequestHandler<GetUnreadNotificationsQuery, IEnumerable<NotificationDto>>
    {
        private readonly INotificationRepository _repository;

        public GetUnreadNotificationsQueryHandler(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<NotificationDto>> Handle(GetUnreadNotificationsQuery request, CancellationToken cancellationToken)
        {
            var notifications = await _repository.GetUnreadByUserIdAsync(request.UserId, cancellationToken);

            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                ImageUrl = n.ImageUrl,
                ActionUrl = n.ActionUrl,
                RelatedEntityId = n.RelatedEntityId,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt ?? DateTime.UtcNow,
                Metadata = n.Metadata
            });
        }
    }

    public class GetNotificationCountQueryHandler : IRequestHandler<GetNotificationCountQuery, NotificationCountDto>
    {
        private readonly INotificationRepository _repository;

        public GetNotificationCountQueryHandler(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<NotificationCountDto> Handle(GetNotificationCountQuery request, CancellationToken cancellationToken)
        {
            var unreadCount = await _repository.GetUnreadCountAsync(request.UserId, cancellationToken);
            var allNotifications = await _repository.GetByUserIdAsync(request.UserId, 0, 1000, cancellationToken);

            return new NotificationCountDto
            {
                UnreadCount = unreadCount,
                TotalCount = allNotifications.Count()
            };
        }
    }
}
