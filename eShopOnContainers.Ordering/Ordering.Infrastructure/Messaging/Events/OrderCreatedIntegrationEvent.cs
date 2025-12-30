using System;

namespace eShopOnContainers.IntegrationEvents
{
    /// <summary>
    /// Integration Event publié quand une commande est créée
    /// </summary>
    public record OrderCreatedIntegrationEvent
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
        /// Montant total de la commande
        /// </summary>
        public decimal TotalAmount { get; init; }

        /// <summary>
        /// Status de la commande
        /// </summary>
        public string OrderStatus { get; init; } = string.Empty;

        /// <summary>
        /// Date de création de la commande (UTC)
        /// </summary>
        public DateTime CreatedAt { get; init; }

        public OrderCreatedIntegrationEvent()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public OrderCreatedIntegrationEvent(
            Guid orderId,
            Guid customerId,
            decimal totalAmount,
            string orderStatus)
        {
            OrderId = orderId;
            CustomerId = customerId;
            TotalAmount = totalAmount;
            OrderStatus = orderStatus;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
