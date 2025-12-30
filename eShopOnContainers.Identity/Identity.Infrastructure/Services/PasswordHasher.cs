using System.Text.RegularExpressions;
using Identity.Application.Common.Interfaces;

namespace Identity.Infrastructure.Services
{
    /// <summary>
    /// Service for hashing and verifying passwords using BCrypt
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// Hashes a plain text password using BCrypt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>BCrypt hashed password</returns>
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            // Generate a salt and hash the password using BCrypt
            // WorkFactor 12 provides a good balance between security and performance
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        /// <summary>
        /// Verifies a plain text password against a BCrypt hash
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hashedPassword">BCrypt hashed password</param>
        /// <returns>True if password matches hash, false otherwise</returns>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            try
            {
                // Verify the password against the hash using BCrypt
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                // If verification fails (invalid hash format, etc.), return false
                return false;
            }
        }

        /// <summary>
        /// Checks if a password meets minimum strength requirements
        /// Requirements:
        /// - At least 8 characters long
        /// - Contains at least one uppercase letter
        /// - Contains at least one lowercase letter
        /// - Contains at least one digit
        /// - Contains at least one special character
        /// </summary>
        /// <param name="password">Plain text password to validate</param>
        /// <returns>True if password meets requirements, false otherwise</returns>
        public bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Minimum length: 8 characters
            if (password.Length < 8)
                return false;

            // Must contain at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // Must contain at least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            // Must contain at least one digit
            if (!Regex.IsMatch(password, @"\d"))
                return false;

            // Must contain at least one special character
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]"))
                return false;

            return true;
        }
    }
}
