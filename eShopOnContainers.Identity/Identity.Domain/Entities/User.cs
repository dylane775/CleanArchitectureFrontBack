using System;
using System.Collections.Generic;
using System.Linq;
using Identity.Domain.Common;
using Identity.Domain.Events;
using Identity.Domain.Exceptions;

namespace Identity.Domain.Entities
{
    /// <summary>
    /// Represents a user in the identity system
    /// </summary>
    public class User : BaseEntity
    {
        // ====== PROPERTIES ======
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string? PhoneNumber { get; private set; }
        public bool IsEmailConfirmed { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public string EmailConfirmationToken { get; private set; } = string.Empty;
        public DateTime? EmailConfirmationTokenExpiresAt { get; private set; }
        public string? PasswordResetToken { get; private set; }
        public DateTime? PasswordResetTokenExpiresAt { get; private set; }

        // Navigation properties
        private readonly List<RefreshToken> _refreshTokens = new();
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        private readonly List<Role> _roles = new();
        public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

        // ====== CONSTRUCTORS ======
        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected User()
        {
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="email">User's email address (must be unique)</param>
        /// <param name="passwordHash">BCrypt hashed password</param>
        /// <param name="firstName">User's first name</param>
        /// <param name="lastName">User's last name</param>
        /// <param name="phoneNumber">User's phone number (optional)</param>
        public User(
            string email,
            string passwordHash,
            string firstName,
            string lastName,
            string? phoneNumber = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new IdentityDomainException("Email cannot be empty");

            if (!IsValidEmail(email))
                throw new IdentityDomainException("Invalid email format");

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new IdentityDomainException("Password hash cannot be empty");

            if (string.IsNullOrWhiteSpace(firstName))
                throw new IdentityDomainException("First name cannot be empty");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new IdentityDomainException("Last name cannot be empty");

            Email = email.ToLowerInvariant().Trim();
            PasswordHash = passwordHash;
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            PhoneNumber = phoneNumber?.Trim();
            IsEmailConfirmed = false;
            IsActive = true;
            LastLoginAt = null;

            // Generate email confirmation token (valid for 24 hours)
            EmailConfirmationToken = GenerateSecureToken();
            EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24);

            // Raise domain event
            AddDomainEvent(new UserRegisteredDomainEvent(Id, Email, FirstName, LastName));
        }

        // ====== BUSINESS METHODS ======

        /// <summary>
        /// Updates the user's profile information
        /// </summary>
        /// <param name="firstName">New first name</param>
        /// <param name="lastName">New last name</param>
        /// <param name="phoneNumber">New phone number (optional)</param>
        public void UpdateProfile(string firstName, string lastName, string? phoneNumber = null)
        {
            if (!IsActive)
                throw new IdentityDomainException("Cannot update profile of an inactive user");

            if (string.IsNullOrWhiteSpace(firstName))
                throw new IdentityDomainException("First name cannot be empty");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new IdentityDomainException("Last name cannot be empty");

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            PhoneNumber = phoneNumber?.Trim();

            UpdateAuditInfo("system");
        }

        /// <summary>
        /// Changes the user's password
        /// </summary>
        /// <param name="newPasswordHash">New BCrypt hashed password</param>
        public void ChangePassword(string newPasswordHash)
        {
            if (!IsActive)
                throw new IdentityDomainException("Cannot change password of an inactive user");

            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new IdentityDomainException("Password hash cannot be empty");

            PasswordHash = newPasswordHash;
            UpdateAuditInfo("system");

            // Revoke all existing refresh tokens for security
            RevokeAllRefreshTokens("Password changed");
        }

        /// <summary>
        /// Confirms the user's email address with token validation
        /// </summary>
        /// <param name="token">Confirmation token sent to user's email</param>
        public void ConfirmEmail(string token)
        {
            if (IsEmailConfirmed)
                throw new IdentityDomainException("Email is already confirmed");

            if (string.IsNullOrWhiteSpace(token))
                throw new IdentityDomainException("Confirmation token cannot be empty");

            if (EmailConfirmationToken != token)
                throw new IdentityDomainException("Invalid confirmation token");

            if (EmailConfirmationTokenExpiresAt.HasValue && EmailConfirmationTokenExpiresAt.Value < DateTime.UtcNow)
                throw new IdentityDomainException("Confirmation token has expired");

            IsEmailConfirmed = true;
            EmailConfirmationToken = string.Empty;
            EmailConfirmationTokenExpiresAt = null;
            UpdateAuditInfo("system");

            // Raise domain event
            AddDomainEvent(new EmailConfirmedDomainEvent(Id, Email));
        }

        /// <summary>
        /// Generates a new email confirmation token
        /// </summary>
        public void RegenerateEmailConfirmationToken()
        {
            if (IsEmailConfirmed)
                throw new IdentityDomainException("Email is already confirmed");

            EmailConfirmationToken = GenerateSecureToken();
            EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
            UpdateAuditInfo("system");
        }

        /// <summary>
        /// Initiates password reset by generating a reset token
        /// </summary>
        public void InitiatePasswordReset()
        {
            if (!IsActive)
                throw new IdentityDomainException("Cannot reset password for an inactive user");

            PasswordResetToken = GenerateSecureToken();
            PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1); // Valid for 1 hour
            UpdateAuditInfo("system");
        }

        /// <summary>
        /// Resets password with token validation
        /// </summary>
        /// <param name="token">Reset token sent to user's email</param>
        /// <param name="newPasswordHash">New BCrypt hashed password</param>
        public void ResetPassword(string token, string newPasswordHash)
        {
            if (!IsActive)
                throw new IdentityDomainException("Cannot reset password for an inactive user");

            if (string.IsNullOrWhiteSpace(token))
                throw new IdentityDomainException("Reset token cannot be empty");

            if (string.IsNullOrWhiteSpace(PasswordResetToken))
                throw new IdentityDomainException("No password reset has been initiated");

            if (PasswordResetToken != token)
                throw new IdentityDomainException("Invalid reset token");

            if (PasswordResetTokenExpiresAt.HasValue && PasswordResetTokenExpiresAt.Value < DateTime.UtcNow)
                throw new IdentityDomainException("Reset token has expired");

            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new IdentityDomainException("Password hash cannot be empty");

            PasswordHash = newPasswordHash;
            PasswordResetToken = null;
            PasswordResetTokenExpiresAt = null;
            UpdateAuditInfo("system");

            // Revoke all existing refresh tokens for security
            RevokeAllRefreshTokens("Password reset");
        }

        /// <summary>
        /// Activates the user account
        /// </summary>
        public void Activate()
        {
            if (IsActive)
                throw new IdentityDomainException("User is already active");

            IsActive = true;
            UpdateAuditInfo("system");
        }

        /// <summary>
        /// Deactivates the user account
        /// </summary>
        public void Deactivate()
        {
            if (!IsActive)
                throw new IdentityDomainException("User is already inactive");

            IsActive = false;
            UpdateAuditInfo("system");

            // Revoke all refresh tokens when deactivating
            RevokeAllRefreshTokens("User deactivated");
        }

        /// <summary>
        /// Records a successful login
        /// </summary>
        /// <param name="ipAddress">IP address of the login</param>
        public void RecordLogin(string ipAddress)
        {
            if (!IsActive)
                throw new IdentityDomainException("Cannot login with an inactive account");

            if (!IsEmailConfirmed)
                throw new IdentityDomainException("Email must be confirmed before login");

            LastLoginAt = DateTime.UtcNow;
            UpdateAuditInfo("system");

            // Raise domain event
            AddDomainEvent(new UserLoggedInDomainEvent(Id, Email, ipAddress, LastLoginAt.Value));
        }

        /// <summary>
        /// Adds a refresh token to the user
        /// </summary>
        /// <param name="token">The refresh token string</param>
        /// <param name="expiresAt">Token expiration date</param>
        /// <param name="createdByIp">IP address that created the token</param>
        public RefreshToken AddRefreshToken(string token, DateTime expiresAt, string createdByIp)
        {
            if (!IsActive)
                throw new IdentityDomainException("Cannot add refresh token to an inactive user");

            var refreshToken = new RefreshToken(token, Id, expiresAt, createdByIp);
            _refreshTokens.Add(refreshToken);

            return refreshToken;
        }

        /// <summary>
        /// Revokes a specific refresh token
        /// </summary>
        /// <param name="token">Token to revoke</param>
        /// <param name="ipAddress">IP address that revoked the token</param>
        /// <param name="replacedByToken">Optional replacement token</param>
        public void RevokeRefreshToken(string token, string ipAddress, string? replacedByToken = null)
        {
            var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);

            if (refreshToken == null)
                throw new IdentityDomainException("Refresh token not found");

            refreshToken.Revoke(ipAddress, replacedByToken);
        }

        /// <summary>
        /// Revokes all refresh tokens for the user
        /// </summary>
        /// <param name="reason">Reason for revoking all tokens</param>
        public void RevokeAllRefreshTokens(string reason)
        {
            foreach (var token in _refreshTokens.Where(t => t.IsActive))
            {
                token.Revoke(reason, null);
            }
        }

        /// <summary>
        /// Adds a role to the user
        /// </summary>
        /// <param name="role">Role to add</param>
        public void AddRole(Role role)
        {
            if (role == null)
                throw new IdentityDomainException("Role cannot be null");

            if (_roles.Any(r => r.Id == role.Id))
                throw new IdentityDomainException($"User already has role {role.Name}");

            _roles.Add(role);
            UpdateAuditInfo("system");
        }

        /// <summary>
        /// Removes a role from the user
        /// </summary>
        /// <param name="role">Role to remove</param>
        public void RemoveRole(Role role)
        {
            if (role == null)
                throw new IdentityDomainException("Role cannot be null");

            var existingRole = _roles.FirstOrDefault(r => r.Id == role.Id);

            if (existingRole == null)
                throw new IdentityDomainException($"User does not have role {role.Name}");

            _roles.Remove(existingRole);
            UpdateAuditInfo("system");
        }

        /// <summary>
        /// Checks if the user has a specific role
        /// </summary>
        /// <param name="roleName">Name of the role to check</param>
        /// <returns>True if user has the role, false otherwise</returns>
        public bool HasRole(string roleName)
        {
            return _roles.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the full name of the user
        /// </summary>
        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }

        /// <summary>
        /// Adds a domain event to the entity
        /// </summary>
        private void AddDomainEvent(BaseDomainEvent domainEvent)
        {
            // This would be implemented in BaseEntity if it supports domain events
            // For now, this is a placeholder for the pattern
        }

        // ====== VALIDATION HELPERS ======

        /// <summary>
        /// Validates email format
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a cryptographically secure random token
        /// </summary>
        private static string GenerateSecureToken()
        {
            // Generate 32 random bytes and convert to Base64 URL-safe string
            var randomBytes = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // Convert to Base64 and make URL-safe
            return Convert.ToBase64String(randomBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}
