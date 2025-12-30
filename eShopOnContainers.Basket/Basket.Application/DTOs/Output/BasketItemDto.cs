using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.Application.DTOs.Output
{
    public record BasketItemDto
    {
        public Guid Id { get; init; }
        public Guid CatalogItemId { get; init; }
        public string ProductName { get; init; }
        public decimal UnitPrice { get; init; }
        public int Quantity { get; init; }
        public decimal TotalPrice { get; init; }
        public string PictureUrl { get; init; }
    }
}