using System;

namespace Ordering.Application.DTOs.Output
{
    /// <summary>
    /// DTO simplifié pour les listes de commandes (sans les détails des items)
    /// </summary>
    public record OrderSummaryDto
    {
        public Guid Id { get; init; }
        public Guid CustomerId { get; init; }
        public string OrderStatus { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public DateTime OrderDate { get; init; }
        public DateTime? DeliveryDate { get; init; }
        public int ItemCount { get; init; }
        public string CustomerEmail { get; init; } = string.Empty;
    }
}
