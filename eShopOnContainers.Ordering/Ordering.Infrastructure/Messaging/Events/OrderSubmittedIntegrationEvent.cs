using System;

namespace eShopOnContainers.IntegrationEvents
{
    /// <summary>
    /// Integration Event publié quand une commande est soumise pour traitement
    /// </summary>
    public record OrderSubmittedIntegrationEvent
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
        /// Nombre total d'articles dans la commande
        /// </summary>
        public int ItemCount { get; init; }

        /// <summary>
        /// Date de soumission (UTC)
        /// </summary>
        public DateTime SubmittedAt { get; init; }

        public OrderSubmittedIntegrationEvent()
        {
            SubmittedAt = DateTime.UtcNow;
        }

        public OrderSubmittedIntegrationEvent(
            Guid orderId,
            Guid customerId,
            decimal totalAmount,
            int itemCount)
        {
            OrderId = orderId;
            CustomerId = customerId;
            TotalAmount = totalAmount;
            ItemCount = itemCount;
            SubmittedAt = DateTime.UtcNow;
        }
    }
}
