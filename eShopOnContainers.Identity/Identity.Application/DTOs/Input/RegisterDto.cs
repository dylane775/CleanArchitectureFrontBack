namespace Identity.Application.DTOs.Input
{
    /// <summary>
    /// Data transfer object for user registration
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's password
        /// </summary>
        public string Password { get; set; } = string.Empty;

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
