using MediatR;

namespace Identity.Application.Commands.ConfirmEmail
{
    /// <summary>
    /// Command to confirm a user's email address
    /// </summary>
    public record ConfirmEmailCommand : IRequest<bool>
    {
        /// <summary>
        /// User's unique identifier
        /// </summary>
        public Guid UserId { get; init; }

        /// <summary>
        /// Email confirmation token
        /// </summary>
        public string ConfirmationToken { get; init; } = string.Empty;
    }
}
