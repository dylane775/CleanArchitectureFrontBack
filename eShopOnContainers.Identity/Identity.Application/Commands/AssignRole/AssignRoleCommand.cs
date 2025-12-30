using MediatR;

namespace Identity.Application.Commands.AssignRole
{
    /// <summary>
    /// Command to assign a role to a user
    /// </summary>
    public record AssignRoleCommand : IRequest<bool>
    {
        /// <summary>
        /// User ID
        /// </summary>
        public Guid UserId { get; init; }

        /// <summary>
        /// Role name to assign
        /// </summary>
        public string RoleName { get; init; } = string.Empty;
    }
}
