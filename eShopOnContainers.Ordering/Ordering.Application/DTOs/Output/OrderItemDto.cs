using System;

namespace Ordering.Application.DTOs.Output
{
    public record OrderItemDto
    {
        public Guid Id { get; init; }
        public Guid CatalogItemId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }
        public int Quantity { get; init; }
        public string? PictureUrl { get; init; }
        public decimal Discount { get; init; }
        public decimal Subtotal { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal TotalPrice { get; init; }
    }
}
