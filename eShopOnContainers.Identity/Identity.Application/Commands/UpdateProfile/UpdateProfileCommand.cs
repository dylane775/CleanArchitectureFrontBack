using MediatR;

namespace Identity.Application.Commands.UpdateProfile
{
    /// <summary>
    /// Command to update a user's profile information
    /// </summary>
    public record UpdateProfileCommand : IRequest<bool>
    {
        /// <summary>
        /// User's unique identifier
        /// </summary>
        public Guid UserId { get; init; }

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
