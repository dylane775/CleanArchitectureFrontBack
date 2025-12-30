using Identity.Application.DTOs.Output;
using MediatR;

namespace Identity.Application.Queries.GetAllUsers
{
    /// <summary>
    /// Query to get all users with optional pagination
    /// </summary>
    public record GetAllUsersQuery : IRequest<List<UserDto>>
    {
        /// <summary>
        /// Page number (1-based)
        /// </summary>
        public int PageNumber { get; init; } = 1;

        /// <summary>
        /// Page size (number of items per page)
        /// </summary>
        public int PageSize { get; init; } = 10;

        /// <summary>
        /// Optional filter by active status
        /// </summary>
        public bool? IsActive { get; init; }
    }
}
