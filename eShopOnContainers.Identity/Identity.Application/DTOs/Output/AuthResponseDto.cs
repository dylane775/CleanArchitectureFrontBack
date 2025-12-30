namespace Identity.Application.DTOs.Output
{
    /// <summary>
    /// Data transfer object for authentication response
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// JWT access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token for obtaining new access tokens
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// User's unique identifier
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// List of role names assigned to the user
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// Date and time when the access token expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
