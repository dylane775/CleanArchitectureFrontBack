using System;
using Identity.Domain.Common;
using Identity.Domain.Exceptions;

namespace Identity.Domain.Entities
{
    /// <summary>
    /// Represents a refresh token for JWT authentication
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        // ====== PROPERTIES ======
        public string Token { get; private set; } = string.Empty;
        public Guid UserId { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public string CreatedByIp { get; private set; } = string.Empty;
        public DateTime? RevokedAt { get; private set; }
        public string? RevokedByIp { get; private set; }
        public string? ReplacedByToken { get; private set; }

        // Navigation property
        public User? User { get; private set; }

        // ====== COMPUTED PROPERTIES ======

        /// <summary>
        /// Checks if the token has expired
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// Checks if the token has been revoked
        /// </summary>
        public bool IsRevoked => RevokedAt.HasValue;

        /// <summary>
        /// Checks if the token is active (not expired and not revoked)
        /// </summary>
        public bool IsActive => !IsRevoked && !IsExpired;

        // ====== CONSTRUCTORS ======

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected RefreshToken()
        {
        }

        /// <summary>
        /// Creates a new refresh token
        /// </summary>
        /// <param name="token">Unique token string</param>
        /// <param name="userId">ID of the user this token belongs to</param>
        /// <param name="expiresAt">Token expiration date</param>
        /// <param name="createdByIp">IP address that created the token</param>
        public RefreshToken(string token, Guid userId, DateTime expiresAt, string createdByIp) : base()
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new IdentityDomainException("Token cannot be empty");

            if (userId == Guid.Empty)
                throw new IdentityDomainException("User ID cannot be empty");

            if (expiresAt <= DateTime.UtcNow)
                throw new IdentityDomainException("Expiration date must be in the future");

            if (string.IsNullOrWhiteSpace(createdByIp))
                throw new IdentityDomainException("IP address cannot be empty");

            Token = token;
            UserId = userId;
            ExpiresAt = expiresAt;
            CreatedByIp = createdByIp.Trim();
            RevokedAt = null;
            RevokedByIp = null;
            ReplacedByToken = null;
        }

        // ====== BUSINESS METHODS ======

        /// <summary>
        /// Revokes the refresh token
        /// </summary>
        /// <param name="ipAddress">IP address that revoked the token</param>
        /// <param name="replacedByToken">Optional token that replaces this one</param>
        public void Revoke(string ipAddress, string? replacedByToken = null)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new IdentityDomainException("IP address cannot be empty");

            if (IsRevoked)
                throw new IdentityDomainException("Token is already revoked");

            RevokedAt = DateTime.UtcNow;
            RevokedByIp = ipAddress.Trim();
            ReplacedByToken = replacedByToken;

            UpdateAuditInfo("system");
        }

        /// <summary>
        /// Validates if the token can be used
        /// </summary>
        /// <returns>True if token is valid and active, false otherwise</returns>
        public bool CanBeUsed()
        {
            return IsActive;
        }

        /// <summary>
        /// Gets the remaining time until expiration
        /// </summary>
        /// <returns>TimeSpan until expiration, or TimeSpan.Zero if expired</returns>
        public TimeSpan GetRemainingTime()
        {
            if (IsExpired)
                return TimeSpan.Zero;

            return ExpiresAt - DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the token will expire soon (within specified threshold)
        /// </summary>
        /// <param name="threshold">Threshold timespan</param>
        /// <returns>True if token will expire within threshold, false otherwise</returns>
        public bool WillExpireSoon(TimeSpan threshold)
        {
            if (IsExpired)
                return true;

            var remainingTime = GetRemainingTime();
            return remainingTime <= threshold;
        }
    }
}
