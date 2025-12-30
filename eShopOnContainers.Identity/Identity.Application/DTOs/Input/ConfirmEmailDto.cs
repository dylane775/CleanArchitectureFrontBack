namespace Identity.Application.DTOs.Input
{
    /// <summary>
    /// Data transfer object for confirming email
    /// </summary>
    public class ConfirmEmailDto
    {
        /// <summary>
        /// User's unique identifier
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Email confirmation token
        /// </summary>
        public string ConfirmationToken { get; set; } = string.Empty;
    }
}
