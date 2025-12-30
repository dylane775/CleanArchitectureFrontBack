namespace Identity.Infrastructure.Services
{
    /// <summary>
    /// Configuration settings for email service
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// Email provider type: SMTP, SendGrid, etc.
        /// </summary>
        public string Provider { get; set; } = "SMTP";

        /// <summary>
        /// Email address that appears in the "From" field
        /// </summary>
        public string FromEmail { get; set; } = string.Empty;

        /// <summary>
        /// Name that appears in the "From" field
        /// </summary>
        public string FromName { get; set; } = "eShop Identity Service";

        // ====================================
        // SMTP SETTINGS
        // ====================================

        /// <summary>
        /// SMTP server host (e.g., smtp.gmail.com)
        /// </summary>
        public string SmtpHost { get; set; } = string.Empty;

        /// <summary>
        /// SMTP server port (usually 587 for TLS)
        /// </summary>
        public int SmtpPort { get; set; } = 587;

        /// <summary>
        /// SMTP username
        /// </summary>
        public string SmtpUsername { get; set; } = string.Empty;

        /// <summary>
        /// SMTP password
        /// </summary>
        public string SmtpPassword { get; set; } = string.Empty;

        /// <summary>
        /// Enable SSL/TLS
        /// </summary>
        public bool EnableSsl { get; set; } = true;

        // ====================================
        // SENDGRID SETTINGS
        // ====================================

        /// <summary>
        /// SendGrid API Key
        /// </summary>
        public string SendGridApiKey { get; set; } = string.Empty;

        // ====================================
        // APPLICATION URLS
        // ====================================

        /// <summary>
        /// Base URL for email confirmation links
        /// </summary>
        public string ConfirmEmailUrl { get; set; } = "http://localhost:5245/api/Auth/confirm-email";

        /// <summary>
        /// Base URL for password reset links
        /// </summary>
        public string ResetPasswordUrl { get; set; } = "http://localhost:5245/api/Auth/reset-password";
    }
}
