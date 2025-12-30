using Identity.Application.DTOs.Output;
using MediatR;

namespace Identity.Application.Queries.GetRoles
{
    /// <summary>
    /// Query to get all available roles
    /// </summary>
    public record GetRolesQuery : IRequest<List<RoleDto>>
    {
    }
}
