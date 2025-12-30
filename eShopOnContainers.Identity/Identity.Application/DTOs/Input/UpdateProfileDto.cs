namespace Identity.Application.DTOs.Input
{
    /// <summary>
    /// Data transfer object for updating user profile
    /// </summary>
    public class UpdateProfileDto
    {
        /// <summary>
        /// User's unique identifier
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// User's phone number (optional)
        /// </summary>
        public string? PhoneNumber { get; set; }
    }
}
