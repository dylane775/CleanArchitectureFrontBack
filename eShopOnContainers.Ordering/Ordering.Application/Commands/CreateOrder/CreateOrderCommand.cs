using System;
using System.Collections.Generic;
using MediatR;
using Ordering.Application.DTOs.Input;

namespace Ordering.Application.Commands.CreateOrder
{
    public record CreateOrderCommand : IRequest<Guid>
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
