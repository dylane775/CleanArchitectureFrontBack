using System.Threading.Tasks;

namespace Identity.Application.Common.Interfaces
{
    /// <summary>
    /// Service for sending emails
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email confirmation message
        /// </summary>
        Task SendEmailConfirmationAsync(string email, string firstName, string confirmationToken, string userId);

        /// <summary>
        /// Sends a password reset email
        /// </summary>
        Task SendPasswordResetAsync(string email, string firstName, string resetToken);

        /// <summary>
        /// Sends a welcome email after email confirmation
        /// </summary>
        Task SendWelcomeEmailAsync(string email, string firstName);

        /// <summary>
        /// Sends a generic email
        /// </summary>
        Task SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null);
    }
}
