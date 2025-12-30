using AutoMapper;
using Identity.Application.Common.Interfaces;
using Identity.Application.DTOs.Output;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Queries.GetUserRoles
{
    /// <summary>
    /// Handler for GetUserRolesQuery
    /// </summary>
    public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, List<RoleDto>>
    {
        private readonly IIdentityDbContext _context;
        private readonly IMapper _mapper;

        public GetUserRolesQueryHandler(
            IIdentityDbContext context,
            IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<RoleDto>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{request.UserId}' not found");
            }

            return _mapper.Map<List<RoleDto>>(user.Roles.ToList());
        }
    }
}
