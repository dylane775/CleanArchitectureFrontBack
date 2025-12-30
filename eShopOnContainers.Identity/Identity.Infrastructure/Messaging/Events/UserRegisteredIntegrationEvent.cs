namespace Identity.Infrastructure.Messaging.Events
{
    /// <summary>
    /// Integration Event published when a new user is registered
    /// This event is consumed by other microservices via RabbitMQ
    /// </summary>
    public record UserRegisteredIntegrationEvent
    {
        /// <summary>
        /// ID of the registered user
        /// </summary>
        public Guid UserId { get; init; }

        /// <summary>
        /// Email address of the user
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// First name of the user
        /// </summary>
        public string FirstName { get; init; } = string.Empty;

        /// <summary>
        /// Last name of the user
        /// </summary>
        public string LastName { get; init; } = string.Empty;

        /// <summary>
        /// Date and time when the user was registered (UTC)
        /// </summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public UserRegisteredIntegrationEvent()
        {
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        public UserRegisteredIntegrationEvent(
            Guid userId,
            string email,
            string firstName,
            string lastName)
        {
            UserId = userId;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
