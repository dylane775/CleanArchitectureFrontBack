using MediatR;

namespace Identity.Application.Commands.Register
{
    /// <summary>
    /// Command to register a new user
    /// </summary>
    public record RegisterCommand : IRequest<Guid>
    {
        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// User's password
        /// </summary>
        public string Password { get; init; } = string.Empty;

        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; init; } = string.Empty;

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; init; } = string.Empty;

        /// <summary>
        /// User's phone number (optional)
        /// </summary>
        public string? PhoneNumber { get; init; }
    }
}
