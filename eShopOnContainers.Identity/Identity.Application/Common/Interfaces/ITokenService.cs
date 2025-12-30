using Identity.Domain.Entities;

namespace Identity.Application.Common.Interfaces
{
    /// <summary>
    /// Service for generating and validating JWT tokens
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT access token for the user
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>JWT token string</returns>
        string GenerateAccessToken(User user);

        /// <summary>
        /// Generates a refresh token
        /// </summary>
        /// <returns>Refresh token string</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Validates a JWT access token
        /// </summary>
        /// <param name="token">Token to validate</param>
        /// <returns>User ID if valid, null otherwise</returns>
        Guid? ValidateAccessToken(string token);

        /// <summary>
        /// Gets the expiration time for access tokens (in minutes)
        /// </summary>
        int GetAccessTokenExpirationMinutes();

        /// <summary>
        /// Gets the expiration time for refresh tokens (in days)
        /// </summary>
        int GetRefreshTokenExpirationDays();
    }
}
