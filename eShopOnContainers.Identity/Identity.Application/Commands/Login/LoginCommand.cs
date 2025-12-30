using Identity.Application.DTOs.Output;
using MediatR;

namespace Identity.Application.Commands.Login
{
    /// <summary>
    /// Command to authenticate a user and generate tokens
    /// </summary>
    public record LoginCommand : IRequest<AuthResponseDto>
    {
        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// User's password
        /// </summary>
        public string Password { get; init; } = string.Empty;

        /// <summary>
        /// IP address of the login request
        /// </summary>
        public string IpAddress { get; init; } = string.Empty;
    }
}
