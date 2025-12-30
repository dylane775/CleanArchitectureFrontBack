using System;

namespace Ordering.Application.DTOs.Input
{
    public record UpdateOrderItemQuantityDto
    {
        public Guid CatalogItemId { get; init; }
        public int NewQuantity { get; init; }
    }
}
