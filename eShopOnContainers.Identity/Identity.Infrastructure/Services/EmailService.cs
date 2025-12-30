using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Identity.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Services
{
    /// <summary>
    /// Email service implementation supporting multiple providers
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendEmailConfirmationAsync(string email, string firstName, string confirmationToken, string userId)
        {
            var confirmUrl = $"{_settings.ConfirmEmailUrl}?userId={userId}&token={Uri.EscapeDataString(confirmationToken)}";

            var subject = "Confirmez votre adresse email - eShop";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{
            display: inline-block;
            padding: 12px 24px;
            background-color: #4CAF50;
            color: white;
            text-decoration: none;
            border-radius: 4px;
            margin: 20px 0;
        }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Bienvenue sur eShop!</h1>
        </div>
        <div class='content'>
            <h2>Bonjour {firstName},</h2>
            <p>Merci de vous être inscrit sur eShop. Pour activer votre compte, veuillez confirmer votre adresse email en cliquant sur le bouton ci-dessous:</p>
            <div style='text-align: center;'>
                <a href='{confirmUrl}' class='button'>Confirmer mon email</a>
            </div>
            <p>Si le bouton ne fonctionne pas, copiez et collez ce lien dans votre navigateur:</p>
            <p style='word-break: break-all; color: #4CAF50;'>{confirmUrl}</p>
            <p><strong>Ce lien expirera dans 24 heures.</strong></p>
            <p>Si vous n'avez pas créé de compte, vous pouvez ignorer cet email.</p>
        </div>
        <div class='footer'>
            <p>© 2025 eShop. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";

            var plainTextBody = $@"
Bonjour {firstName},

Merci de vous être inscrit sur eShop. Pour activer votre compte, veuillez confirmer votre adresse email en cliquant sur le lien ci-dessous:

{confirmUrl}

Ce lien expirera dans 24 heures.

Si vous n'avez pas créé de compte, vous pouvez ignorer cet email.

© 2025 eShop. Tous droits réservés.
";

            await SendEmailAsync(email, subject, htmlBody, plainTextBody);
        }

        public async Task SendPasswordResetAsync(string email, string firstName, string resetToken)
        {
            var resetUrl = $"{_settings.ResetPasswordUrl}?token={Uri.EscapeDataString(resetToken)}";

            var subject = "Réinitialisation de votre mot de passe - eShop";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #FF9800; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{
            display: inline-block;
            padding: 12px 24px;
            background-color: #FF9800;
            color: white;
            text-decoration: none;
            border-radius: 4px;
            margin: 20px 0;
        }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #777; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ff9800; padding: 12px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Réinitialisation de mot de passe</h1>
        </div>
        <div class='content'>
            <h2>Bonjour {firstName},</h2>
            <p>Vous avez demandé la réinitialisation de votre mot de passe. Cliquez sur le bouton ci-dessous pour créer un nouveau mot de passe:</p>
            <div style='text-align: center;'>
                <a href='{resetUrl}' class='button'>Réinitialiser mon mot de passe</a>
            </div>
            <p>Si le bouton ne fonctionne pas, copiez et collez ce lien dans votre navigateur:</p>
            <p style='word-break: break-all; color: #FF9800;'>{resetUrl}</p>
            <div class='warning'>
                <strong>⚠️ Important:</strong> Ce lien expirera dans 1 heure pour des raisons de sécurité.
            </div>
            <p>Si vous n'avez pas demandé de réinitialisation, ignorez cet email. Votre mot de passe restera inchangé.</p>
        </div>
        <div class='footer'>
            <p>© 2025 eShop. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";

            var plainTextBody = $@"
Bonjour {firstName},

Vous avez demandé la réinitialisation de votre mot de passe. Cliquez sur le lien ci-dessous pour créer un nouveau mot de passe:

{resetUrl}

⚠️ Important: Ce lien expirera dans 1 heure pour des raisons de sécurité.

Si vous n'avez pas demandé de réinitialisation, ignorez cet email. Votre mot de passe restera inchangé.

© 2025 eShop. Tous droits réservés.
";

            await SendEmailAsync(email, subject, htmlBody, plainTextBody);
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName)
        {
            var subject = "Bienvenue sur eShop!";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Bienvenue sur eShop!</h1>
        </div>
        <div class='content'>
            <h2>Bonjour {firstName},</h2>
            <p>Félicitations! Votre adresse email a été confirmée avec succès.</p>
            <p>Vous pouvez maintenant profiter de toutes les fonctionnalités de eShop:</p>
            <ul>
                <li>✅ Commander des produits</li>
                <li>✅ Suivre vos commandes</li>
                <li>✅ Gérer votre profil</li>
                <li>✅ Accéder à des offres exclusives</li>
            </ul>
            <p>Merci de nous rejoindre!</p>
        </div>
        <div class='footer'>
            <p>© 2025 eShop. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";

            var plainTextBody = $@"
Bonjour {firstName},

Félicitations! Votre adresse email a été confirmée avec succès.

Vous pouvez maintenant profiter de toutes les fonctionnalités de eShop:
- Commander des produits
- Suivre vos commandes
- Gérer votre profil
- Accéder à des offres exclusives

Merci de nous rejoindre!

© 2025 eShop. Tous droits réservés.
";

            await SendEmailAsync(email, subject, htmlBody, plainTextBody);
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null)
        {
            try
            {
                _logger.LogInformation("Sending email to {Email} with subject: {Subject}", to, subject);

                if (_settings.Provider.Equals("SMTP", StringComparison.OrdinalIgnoreCase))
                {
                    await SendViaSmtpAsync(to, subject, htmlBody, plainTextBody);
                }
                else if (_settings.Provider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
                {
                    await SendViaSendGridAsync(to, subject, htmlBody, plainTextBody);
                }
                else
                {
                    throw new InvalidOperationException($"Email provider '{_settings.Provider}' is not supported");
                }

                _logger.LogInformation("Email sent successfully to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                throw;
            }
        }

        private async Task SendViaSmtpAsync(string to, string subject, string htmlBody, string? plainTextBody)
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword),
                EnableSsl = _settings.EnableSsl
            };

            var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(to);

            // Add plain text alternative if provided
            if (!string.IsNullOrEmpty(plainTextBody))
            {
                var plainView = AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain");
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");
                message.AlternateViews.Add(plainView);
                message.AlternateViews.Add(htmlView);
            }

            await client.SendMailAsync(message);
        }

        private async Task SendViaSendGridAsync(string to, string subject, string htmlBody, string? plainTextBody)
        {
            // TODO: Implement SendGrid integration
            // You'll need to install SendGrid NuGet package and implement this
            _logger.LogWarning("SendGrid provider is not yet implemented. Email not sent.");
            await Task.CompletedTask;
            throw new NotImplementedException("SendGrid provider is not yet implemented");
        }
    }
}
