using AutoMapper;
using Identity.Application.Common.Interfaces;
using Identity.Application.Common.Models;
using Identity.Application.DTOs.Output;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Queries.GetAllUsers
{
    /// <summary>
    /// Handler for GetAllUsersQuery
    /// </summary>
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PaginatedItems<UserDto>>
    {
        private readonly IIdentityDbContext _context;
        private readonly IMapper _mapper;

        public GetAllUsersQueryHandler(
            IIdentityDbContext context,
            IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PaginatedItems<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Users
                .Include(u => u.Roles)
                .AsQueryable();

            // Apply active filter if specified
            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var users = await query
                .OrderBy(u => u.Email)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var userDtos = _mapper.Map<List<UserDto>>(users);

            return new PaginatedItems<UserDto>(
                pageIndex: request.PageNumber,
                pageSize: request.PageSize,
                count: totalCount,
                data: userDtos
            );
        }
    }
}
