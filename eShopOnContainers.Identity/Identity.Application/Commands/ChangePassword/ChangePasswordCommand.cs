using MediatR;

namespace Identity.Application.Commands.ChangePassword
{
    /// <summary>
    /// Command to change a user's password
    /// </summary>
    public record ChangePasswordCommand : IRequest<bool>
    {
        /// <summary>
        /// User's unique identifier
        /// </summary>
        public Guid UserId { get; init; }

        /// <summary>
        /// Current password
        /// </summary>
        public string CurrentPassword { get; init; } = string.Empty;

        /// <summary>
        /// New password
        /// </summary>
        public string NewPassword { get; init; } = string.Empty;
    }
}
