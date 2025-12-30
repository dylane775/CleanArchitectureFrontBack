namespace Identity.Infrastructure.Services
{
    /// <summary>
    /// Configuration settings for JWT token generation and validation
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Secret key for signing JWT tokens
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        /// JWT token issuer
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// JWT token audience
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Access token expiration time in minutes
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Refresh token expiration time in days
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }
}
