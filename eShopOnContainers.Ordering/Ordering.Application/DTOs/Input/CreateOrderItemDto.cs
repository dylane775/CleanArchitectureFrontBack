using System;

namespace Ordering.Application.DTOs.Input
{
    public record CreateOrderItemDto
    {
        public Guid CatalogItemId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }
        public int Quantity { get; init; }
        public string? PictureUrl { get; init; }
        public decimal Discount { get; init; }
    }
}
