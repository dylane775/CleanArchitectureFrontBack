using MediatR;
using Notification.Application.DTOs;
using Notification.Domain.Entities;
using Notification.Domain.Repositories;

namespace Notification.Application.Commands.CreateNotification
{
    public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, NotificationDto>
    {
        private readonly INotificationRepository _repository;

        public CreateNotificationCommandHandler(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<NotificationDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = new UserNotification(
                request.UserId,
                request.Type,
                request.Title,
                request.Message,
                request.ImageUrl,
                request.ActionUrl,
                request.RelatedEntityId,
                request.Metadata
            );

            await _repository.AddAsync(notification, cancellationToken);

            return new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                ImageUrl = notification.ImageUrl,
                ActionUrl = notification.ActionUrl,
                RelatedEntityId = notification.RelatedEntityId,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                CreatedAt = notification.CreatedAt ?? DateTime.UtcNow,
                Metadata = notification.Metadata
            };
        }
    }
}
