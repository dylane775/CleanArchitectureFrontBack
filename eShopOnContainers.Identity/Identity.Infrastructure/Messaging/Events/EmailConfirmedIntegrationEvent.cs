namespace Identity.Infrastructure.Messaging.Events
{
    /// <summary>
    /// Integration Event published when a user confirms their email address
    /// This event is consumed by other microservices via RabbitMQ
    /// </summary>
    public record EmailConfirmedIntegrationEvent
    {
        /// <summary>
        /// ID of the user who confirmed their email
        /// </summary>
        public Guid UserId { get; init; }

        /// <summary>
        /// Email address that was confirmed
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Date and time when the email was confirmed (UTC)
        /// </summary>
        public DateTime ConfirmedAt { get; init; }

        /// <summary>
        /// Date and time when the event was created (UTC)
        /// </summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public EmailConfirmedIntegrationEvent()
        {
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        public EmailConfirmedIntegrationEvent(
            Guid userId,
            string email)
        {
            UserId = userId;
            Email = email;
            ConfirmedAt = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
