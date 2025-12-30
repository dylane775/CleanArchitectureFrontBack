using Identity.Application.DTOs.Output;
using MediatR;

namespace Identity.Application.Queries.GetUserRoles
{
    /// <summary>
    /// Query to get all roles for a specific user
    /// </summary>
    public record GetUserRolesQuery : IRequest<List<RoleDto>>
    {
        /// <summary>
        /// User's unique identifier
        /// </summary>
        public Guid UserId { get; init; }
    }
}
