using System;
using System.Collections.Generic;

namespace Ordering.Application.DTOs.Output
{
    public record OrderDto
    {
        public Guid Id { get; init; }
        public Guid CustomerId { get; init; }
        public string OrderStatus { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public DateTime OrderDate { get; init; }
        public DateTime? DeliveryDate { get; init; }
        public string ShippingAddress { get; init; } = string.Empty;
        public string BillingAddress { get; init; } = string.Empty;
        public string PaymentMethod { get; init; } = string.Empty;
        public string CustomerEmail { get; init; } = string.Empty;
        public string? CustomerPhone { get; init; }

        public List<OrderItemDto> Items { get; init; } = new();

        public int TotalItemCount { get; init; }
        public decimal Subtotal { get; init; }
        public decimal TotalDiscount { get; init; }

        // Audit
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public DateTime? ModifiedAt { get; init; }
        public string? ModifiedBy { get; init; }
    }
}
