using MediatR;
using Notification.Domain.Repositories;

namespace Notification.Application.Commands.DeleteNotification
{
    public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, bool>
    {
        private readonly INotificationRepository _repository;

        public DeleteNotificationCommandHandler(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = await _repository.GetByIdAsync(request.NotificationId, cancellationToken);

            if (notification == null)
                return false;

            await _repository.DeleteAsync(request.NotificationId, cancellationToken);
            return true;
        }
    }

    public class DeleteAllNotificationsCommandHandler : IRequestHandler<DeleteAllNotificationsCommand, bool>
    {
        private readonly INotificationRepository _repository;

        public DeleteAllNotificationsCommandHandler(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteAllNotificationsCommand request, CancellationToken cancellationToken)
        {
            await _repository.DeleteAllByUserIdAsync(request.UserId, cancellationToken);
            return true;
        }
    }
}
