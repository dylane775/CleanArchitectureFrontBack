using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.Common.Interfaces;
using Identity.Application.DTOs.Output;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Queries.GetRoles
{
    /// <summary>
    /// Handler for GetRolesQuery
    /// </summary>
    public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
    {
        private readonly IIdentityDbContext _context;

        public GetRolesQueryHandler(IIdentityDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _context.Roles
                .OrderBy(r => r.Name)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Permissions = r.Permissions != null ? r.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>()
                })
                .ToListAsync(cancellationToken);

            return roles;
        }
    }
}
