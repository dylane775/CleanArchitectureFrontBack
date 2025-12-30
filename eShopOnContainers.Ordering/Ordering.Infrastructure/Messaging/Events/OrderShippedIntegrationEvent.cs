using System;

namespace eShopOnContainers.IntegrationEvents
{
    /// <summary>
    /// Integration Event publié quand une commande est expédiée
    /// </summary>
    public record OrderShippedIntegrationEvent
    {
        /// <summary>
        /// ID de la commande
        /// </summary>
        public Guid OrderId { get; init; }

        /// <summary>
        /// ID du client
        /// </summary>
        public Guid CustomerId { get; init; }

        /// <summary>
        /// Adresse de livraison
        /// </summary>
        public string ShippingAddress { get; init; } = string.Empty;

        /// <summary>
        /// Date d'expédition (UTC)
        /// </summary>
        public DateTime ShippedAt { get; init; }

        public OrderShippedIntegrationEvent()
        {
            ShippedAt = DateTime.UtcNow;
        }

        public OrderShippedIntegrationEvent(
            Guid orderId,
            Guid customerId,
            string shippingAddress)
        {
            OrderId = orderId;
            CustomerId = customerId;
            ShippingAddress = shippingAddress;
            ShippedAt = DateTime.UtcNow;
        }
    }
}
