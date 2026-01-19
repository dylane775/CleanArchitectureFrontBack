namespace Notification.Application.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ActionUrl { get; set; }
        public string? RelatedEntityId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class CreateNotificationDto
    {
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ActionUrl { get; set; }
        public string? RelatedEntityId { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class NotificationCountDto
    {
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
    }
}
