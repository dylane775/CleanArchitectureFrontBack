using System;
using System.Collections.Generic;

namespace Ordering.Application.DTOs.Input
{
    public record CreateOrderDto
    {
        public Guid CustomerId { get; init; }
        public string ShippingAddress { get; init; } = string.Empty;
        public string BillingAddress { get; init; } = string.Empty;
        public string PaymentMethod { get; init; } = string.Empty;
        public string CustomerEmail { get; init; } = string.Empty;
        public string? CustomerPhone { get; init; }
        public List<CreateOrderItemDto> Items { get; init; } = new();
    }
}
