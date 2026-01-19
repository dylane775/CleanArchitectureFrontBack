using Notification.Domain.Common;

namespace Notification.Domain.Entities
{
    public class UserNotification : Entity
    {
        public Guid UserId { get; private set; }
        public string Type { get; private set; } = string.Empty;
        public string Title { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;
        public string? ImageUrl { get; private set; }
        public string? ActionUrl { get; private set; }
        public string? RelatedEntityId { get; private set; }
        public bool IsRead { get; private set; }
        public DateTime? ReadAt { get; private set; }
        public Dictionary<string, string>? Metadata { get; private set; }

        protected UserNotification() { }

        public UserNotification(
            Guid userId,
            string type,
            string title,
            string message,
            string? imageUrl = null,
            string? actionUrl = null,
            string? relatedEntityId = null,
            Dictionary<string, string>? metadata = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type cannot be empty", nameof(type));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty", nameof(title));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty", nameof(message));

            UserId = userId;
            Type = type;
            Title = title;
            Message = message;
            ImageUrl = imageUrl;
            ActionUrl = actionUrl;
            RelatedEntityId = relatedEntityId;
            Metadata = metadata;
            IsRead = false;

            SetCreated();
        }

        public void MarkAsRead()
        {
            if (!IsRead)
            {
                IsRead = true;
                ReadAt = DateTime.UtcNow;
                SetModified();
            }
        }

        public void MarkAsUnread()
        {
            if (IsRead)
            {
                IsRead = false;
                ReadAt = null;
                SetModified();
            }
        }
    }
}
