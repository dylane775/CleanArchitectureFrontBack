using MediatR;

namespace Identity.Application.Commands.RemoveRole
{
    /// <summary>
    /// Command to remove a role from a user
    /// </summary>
    public record RemoveRoleCommand : IRequest<bool>
    {
        /// <summary>
        /// User ID
        /// </summary>
        public Guid UserId { get; init; }

        /// <summary>
        /// Role name to remove
        /// </summary>
        public string RoleName { get; init; } = string.Empty;
    }
}
