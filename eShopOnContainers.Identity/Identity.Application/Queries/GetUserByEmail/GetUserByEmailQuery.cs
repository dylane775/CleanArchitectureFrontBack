using Identity.Application.DTOs.Output;
using MediatR;

namespace Identity.Application.Queries.GetUserByEmail
{
    /// <summary>
    /// Query to get a user by their email address
    /// </summary>
    public record GetUserByEmailQuery : IRequest<UserDto?>
    {
        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; init; } = string.Empty;
    }
}
