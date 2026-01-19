using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ordering.Infrastructure.Services
{
    public interface INotificationClient
    {
        Task SendOrderConfirmedNotificationAsync(string userId, Guid orderId, decimal totalAmount);
        Task SendOrderShippedNotificationAsync(string userId, Guid orderId, string shippingAddress);
        Task SendOrderDeliveredNotificationAsync(string userId, Guid orderId);
        Task SendOrderCancelledNotificationAsync(string userId, Guid orderId, string? reason);
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

        public async Task SendOrderConfirmedNotificationAsync(string userId, Guid orderId, decimal totalAmount)
        {
            try
            {
                var notification = new
                {
                    userId = userId,
                    type = "ORDER_CONFIRMED",
                    title = "Commande confirmée",
                    message = $"Votre commande #{orderId.ToString()[..8]} de {totalAmount:N0} XAF a été confirmée et est en cours de préparation.",
                    imageUrl = (string?)null,
                    actionUrl = $"/orders",
                    relatedEntityId = orderId.ToString(),
                    metadata = new Dictionary<string, string>
                    {
                        { "orderId", orderId.ToString() },
                        { "totalAmount", totalAmount.ToString() }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync($"{_notificationApiUrl}/notifications", notification);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Order confirmed notification sent to user {UserId} for order {OrderId}", userId, orderId);
                }
                else
                {
                    _logger.LogWarning("Failed to send order notification. Status: {Status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order confirmed notification for order {OrderId}", orderId);
            }
        }

        public async Task SendOrderShippedNotificationAsync(string userId, Guid orderId, string shippingAddress)
        {
            try
            {
                var notification = new
                {
                    userId = userId,
                    type = "ORDER_SHIPPED",
                    title = "Commande expédiée",
                    message = $"Votre commande #{orderId.ToString()[..8]} a été expédiée ! Elle sera livrée à : {shippingAddress}",
                    imageUrl = (string?)null,
                    actionUrl = $"/orders",
                    relatedEntityId = orderId.ToString(),
                    metadata = new Dictionary<string, string>
                    {
                        { "orderId", orderId.ToString() },
                        { "shippingAddress", shippingAddress }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync($"{_notificationApiUrl}/notifications", notification);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Order shipped notification sent to user {UserId} for order {OrderId}", userId, orderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order shipped notification for order {OrderId}", orderId);
            }
        }

        public async Task SendOrderDeliveredNotificationAsync(string userId, Guid orderId)
        {
            try
            {
                var notification = new
                {
                    userId = userId,
                    type = "ORDER_DELIVERED",
                    title = "Commande livrée",
                    message = $"Votre commande #{orderId.ToString()[..8]} a été livrée avec succès ! Merci pour votre achat.",
                    imageUrl = (string?)null,
                    actionUrl = $"/orders",
                    relatedEntityId = orderId.ToString(),
                    metadata = new Dictionary<string, string>
                    {
                        { "orderId", orderId.ToString() }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync($"{_notificationApiUrl}/notifications", notification);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Order delivered notification sent to user {UserId} for order {OrderId}", userId, orderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order delivered notification for order {OrderId}", orderId);
            }
        }

        public async Task SendOrderCancelledNotificationAsync(string userId, Guid orderId, string? reason)
        {
            try
            {
                var message = string.IsNullOrEmpty(reason)
                    ? $"Votre commande #{orderId.ToString()[..8]} a été annulée."
                    : $"Votre commande #{orderId.ToString()[..8]} a été annulée. Raison : {reason}";

                var notification = new
                {
                    userId = userId,
                    type = "ORDER_CANCELLED",
                    title = "Commande annulée",
                    message = message,
                    imageUrl = (string?)null,
                    actionUrl = $"/orders",
                    relatedEntityId = orderId.ToString(),
                    metadata = new Dictionary<string, string>
                    {
                        { "orderId", orderId.ToString() },
                        { "reason", reason ?? "" }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync($"{_notificationApiUrl}/notifications", notification);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Order cancelled notification sent to user {UserId} for order {OrderId}", userId, orderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order cancelled notification for order {OrderId}", orderId);
            }
        }
    }
}
