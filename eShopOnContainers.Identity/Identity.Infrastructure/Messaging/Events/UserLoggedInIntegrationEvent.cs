namespace Identity.Infrastructure.Messaging.Events
{
    /// <summary>
    /// Integration Event published when a user successfully logs in
    /// This event is consumed by other microservices via RabbitMQ
    /// </summary>
    public record UserLoggedInIntegrationEvent
    {
        /// <summary>
        /// ID of the logged-in user
        /// </summary>
        public Guid UserId { get; init; }

        /// <summary>
        /// Email address of the user
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// IP address from which the user logged in
        /// </summary>
        public string IpAddress { get; init; } = string.Empty;

        /// <summary>
        /// Date and time when the user logged in (UTC)
        /// </summary>
        public DateTime LoginTime { get; init; }

        /// <summary>
        /// Date and time when the event was created (UTC)
        /// </summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public UserLoggedInIntegrationEvent()
        {
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        public UserLoggedInIntegrationEvent(
            Guid userId,
            string email,
            string ipAddress,
            DateTime loginTime)
        {
            UserId = userId;
            Email = email;
            IpAddress = ipAddress;
            LoginTime = loginTime;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
