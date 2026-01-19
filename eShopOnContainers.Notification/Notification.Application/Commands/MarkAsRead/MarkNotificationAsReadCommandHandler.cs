using MediatR;
using Notification.Domain.Repositories;

namespace Notification.Application.Commands.MarkAsRead
{
    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, bool>
    {
        private readonly INotificationRepository _repository;

        public MarkNotificationAsReadCommandHandler(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await _repository.GetByIdAsync(request.NotificationId, cancellationToken);

            if (notification == null)
                return false;

            notification.MarkAsRead();
            await _repository.UpdateAsync(notification, cancellationToken);

            return true;
        }
    }

    public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, bool>
    {
        private readonly INotificationRepository _repository;

        public MarkAllNotificationsAsReadCommandHandler(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
        {
            await _repository.MarkAllAsReadAsync(request.UserId, cancellationToken);
            return true;
        }
    }
}
