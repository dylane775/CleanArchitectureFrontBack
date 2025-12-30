using System;

namespace eShopOnContainers.IntegrationEvents
{
    /// <summary>
    /// Integration Event publié quand une commande est annulée
    /// </summary>
    public record OrderCancelledIntegrationEvent
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
        /// Ancien status de la commande
        /// </summary>
        public string PreviousStatus { get; init; } = string.Empty;

        /// <summary>
        /// Raison de l'annulation
        /// </summary>
        public string? CancellationReason { get; init; }

        /// <summary>
        /// Date d'annulation (UTC)
        /// </summary>
        public DateTime CancelledAt { get; init; }

        public OrderCancelledIntegrationEvent()
        {
            CancelledAt = DateTime.UtcNow;
        }

        public OrderCancelledIntegrationEvent(
            Guid orderId,
            Guid customerId,
            string previousStatus,
            string? cancellationReason = null)
        {
            OrderId = orderId;
            CustomerId = customerId;
            PreviousStatus = previousStatus;
            CancellationReason = cancellationReason;
            CancelledAt = DateTime.UtcNow;
        }
    }
}
