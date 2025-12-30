using System;

namespace Ordering.Application.DTOs.Input
{
    public record ApplyItemDiscountDto
    {
        public Guid CatalogItemId { get; init; }
        public decimal Discount { get; init; }
    }
}
