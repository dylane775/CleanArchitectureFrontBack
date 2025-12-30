namespace Identity.Application.DTOs.Input
{
    /// <summary>
    /// Data transfer object for changing password
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// User's unique identifier
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Current password
        /// </summary>
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// New password
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;
    }
}
