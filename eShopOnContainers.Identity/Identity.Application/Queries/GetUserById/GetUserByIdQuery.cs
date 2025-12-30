using Identity.Application.DTOs.Output;
using MediatR;

namespace Identity.Application.Queries.GetUserById
{
    /// <summary>
    /// Query to get a user by their ID
    /// </summary>
    public record GetUserByIdQuery : IRequest<UserDto?>
    {
        /// <summary>
        /// User's unique identifier
        /// </summary>
        public Guid UserId { get; init; }
    }
}
