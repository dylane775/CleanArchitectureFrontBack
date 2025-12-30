using Identity.Application.DTOs.Output;
using MediatR;

namespace Identity.Application.Commands.RefreshToken
{
    /// <summary>
    /// Command to refresh an access token using a refresh token
    /// </summary>
    public record RefreshTokenCommand : IRequest<AuthResponseDto>
    {
        /// <summary>
        /// Refresh token string
        /// </summary>
        public string RefreshToken { get; init; } = string.Empty;

        /// <summary>
        /// IP address of the refresh request
        /// </summary>
        public string IpAddress { get; init; } = string.Empty;
    }
}
