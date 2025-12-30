using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity.Application.Common.Interfaces;
using Identity.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Services
{
    /// <summary>
    /// Service for generating and validating JWT tokens
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));

            // Validate settings
            if (string.IsNullOrWhiteSpace(_jwtSettings.Secret))
                throw new ArgumentException("JWT Secret cannot be empty");

            if (string.IsNullOrWhiteSpace(_jwtSettings.Issuer))
                throw new ArgumentException("JWT Issuer cannot be empty");

            if (string.IsNullOrWhiteSpace(_jwtSettings.Audience))
                throw new ArgumentException("JWT Audience cannot be empty");
        }

        /// <summary>
        /// Generates a JWT access token for the user with claims (UserId, Email, Roles)
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>JWT token string</returns>
        public string GenerateAccessToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            // Build claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.GetFullName()),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            // Add roles to claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            // Create token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            // Generate token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generates a cryptographically secure random refresh token
        /// </summary>
        /// <returns>Refresh token string</returns>
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Validates a JWT access token
        /// </summary>
        /// <param name="token">Token to validate</param>
        /// <returns>User ID if valid, null otherwise</returns>
        public Guid? ValidateAccessToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

                // Validation parameters
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // No tolerance for expiration
                };

                // Validate token
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                // Extract user ID from claims
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }

                return null;
            }
            catch
            {
                // Token validation failed
                return null;
            }
        }

        /// <summary>
        /// Gets the expiration time for access tokens (in minutes)
        /// </summary>
        public int GetAccessTokenExpirationMinutes()
        {
            return _jwtSettings.AccessTokenExpirationMinutes;
        }

        /// <summary>
        /// Gets the expiration time for refresh tokens (in days)
        /// </summary>
        public int GetRefreshTokenExpirationDays()
        {
            return _jwtSettings.RefreshTokenExpirationDays;
        }
    }
}
