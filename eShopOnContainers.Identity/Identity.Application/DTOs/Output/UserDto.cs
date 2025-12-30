namespace Identity.Application.DTOs.Output
{
    /// <summary>
    /// Data transfer object for User entity
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// User's unique identifier
        /// </summary>
        public Guid Id { get; set; }

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
        /// User's phone number
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Indicates whether the user's email is confirmed
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        /// Indicates whether the user account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// List of role names assigned to the user
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// Date and time when the user was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date and time of the user's last login
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
    }
}
