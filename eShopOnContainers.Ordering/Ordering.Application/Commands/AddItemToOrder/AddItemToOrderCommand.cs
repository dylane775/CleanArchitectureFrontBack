using System;
using MediatR;

namespace Ordering.Application.Commands.AddItemToOrder
{
    public record AddItemToOrderCommand : IRequest<Unit>
    {
        public Guid OrderId { get; init; }
        public Guid CatalogItemId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }
        public int Quantity { get; init; }
        public string? PictureUrl { get; init; }
        public decimal Discount { get; init; }
    }
}
