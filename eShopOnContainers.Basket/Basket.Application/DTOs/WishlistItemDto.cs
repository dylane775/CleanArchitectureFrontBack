using System;

namespace Basket.Application.DTOs
{
    public record WishlistItemDto
    {
        public Guid Id { get; init; }
        public Guid CatalogItemId { get; init; }
        public string ProductName { get; init; } = "";
        public decimal Price { get; init; }
        public string PictureUrl { get; init; } = "";
        public string BrandName { get; init; } = "";
        public string CategoryName { get; init; } = "";
        public DateTime AddedAt { get; init; }
    }

    public record AddToWishlistDto
    {
        public Guid CatalogItemId { get; init; }
        public string ProductName { get; init; } = "";
        public decimal Price { get; init; }
        public string PictureUrl { get; init; } = "";
        public string BrandName { get; init; } = "";
        public string CategoryName { get; init; } = "";
    }
}
