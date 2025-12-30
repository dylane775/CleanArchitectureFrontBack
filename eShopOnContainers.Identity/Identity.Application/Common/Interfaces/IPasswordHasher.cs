namespace Identity.Application.Common.Interfaces
{
    /// <summary>
    /// Service for hashing and verifying passwords using BCrypt
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hashes a plain text password using BCrypt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>BCrypt hashed password</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verifies a plain text password against a BCrypt hash
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hashedPassword">BCrypt hashed password</param>
        /// <returns>True if password matches hash, false otherwise</returns>
        bool VerifyPassword(string password, string hashedPassword);

        /// <summary>
        /// Checks if a password meets minimum strength requirements
        /// </summary>
        /// <param name="password">Plain text password to validate</param>
        /// <returns>True if password meets requirements, false otherwise</returns>
        bool IsPasswordStrong(string password);
    }
}
