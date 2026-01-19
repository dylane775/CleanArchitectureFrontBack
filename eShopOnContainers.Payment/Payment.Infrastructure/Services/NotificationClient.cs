using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Payment.Infrastructure.Services
{
    public interface INotificationClient
    {
        Task SendPaymentConfirmedNotificationAsync(string userId, Guid orderId, decimal amount, string currency);
        Task SendPaymentFailedNotificationAsync(string userId, Guid orderId, string reason);
        Task SendPaymentRefundedNotificationAsync(string userId, Guid orderId, decimal amount);
    }

    public class NotificationClient : INotificationClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationClient> _logger;
        private readonly string _notificationApiUrl;

        public NotificationClient(HttpClient httpClient, IConfiguration configuration, ILogger<NotificationClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _notificationApiUrl = configuration["NotificationSettings:ApiUrl"] ?? "http://localhost:5008/api";
        }

        public async Task SendPaymentConfirmedNotificationAsync(string userId, Guid orderId, decimal amount, string currency)
        {
            try
            {
                var notification = new
                {
                    userId = userId,
                    type = "PAYMENT_RECEIVED",
                    title = "Paiement confirmé",
                    message = $"Votre paiement de {amount:N0} {currency} pour la commande #{orderId.ToString()[..8]} a été reçu avec succès.",
                    imageUrl = (string?)null,
                    actionUrl = $"/orders",
                    relatedEntityId = orderId.ToString(),
                    metadata = new Dictionary<string, string>
                    {
                        { "orderId", orderId.ToString() },
                        { "amount", amount.ToString() },
                        { "currency", currency }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync($"{_notificationApiUrl}/notifications", notification);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Payment confirmed notification sent to user {UserId} for order {OrderId}", userId, orderId);
                }
                else
                {
                    _logger.LogWarning("Failed to send payment notification. Status: {Status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment confirmed notification for order {OrderId}", orderId);
                // Ne pas faire échouer le flux principal si la notification échoue
            }
        }

        public async Task SendPaymentFailedNotificationAsync(string userId, Guid orderId, string reason)
        {
            try
            {
                var notification = new
                {
                    userId = userId,
                    type = "PAYMENT_FAILED",
                    title = "Échec du paiement",
                    message = $"Le paiement pour votre commande #{orderId.ToString()[..8]} a échoué. {reason}",
                    imageUrl = (string?)null,
                    actionUrl = $"/checkout",
                    relatedEntityId = orderId.ToString(),
                    metadata = new Dictionary<string, string>
                    {
                        { "orderId", orderId.ToString() },
                        { "reason", reason }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync($"{_notificationApiUrl}/notifications", notification);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Payment failed notification sent to user {UserId} for order {OrderId}", userId, orderId);
                }
                else
                {
                    _logger.LogWarning("Failed to send payment failed notification. Status: {Status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment failed notification for order {OrderId}", orderId);
            }
        }

        public async Task SendPaymentRefundedNotificationAsync(string userId, Guid orderId, decimal amount)
        {
            try
            {
                var notification = new
                {
                    userId = userId,
                    type = "PAYMENT_RECEIVED",
                    title = "Remboursement effectué",
                    message = $"Un remboursement de {amount:N0} XAF a été effectué pour votre commande #{orderId.ToString()[..8]}.",
                    imageUrl = (string?)null,
                    actionUrl = $"/orders",
                    relatedEntityId = orderId.ToString(),
                    metadata = new Dictionary<string, string>
                    {
                        { "orderId", orderId.ToString() },
                        { "refundAmount", amount.ToString() }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync($"{_notificationApiUrl}/notifications", notification);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Refund notification sent to user {UserId} for order {OrderId}", userId, orderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending refund notification for order {OrderId}", orderId);
            }
        }
    }
}
