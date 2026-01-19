using System;

namespace Catalog.Application.DTOs.Output
{
    public record ProductReviewDto
    {
        public Guid Id { get; init; }
        public Guid CatalogItemId { get; init; }
        public string UserId { get; init; } = string.Empty;
        public string UserDisplayName { get; init; } = string.Empty;
        public int Rating { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Comment { get; init; } = string.Empty;
        public bool IsVerifiedPurchase { get; init; }
        public int HelpfulCount { get; init; }
        public int TotalVotes { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public record ProductReviewStatsDto
    {
        public Guid CatalogItemId { get; init; }
        public int TotalReviews { get; init; }
        public double AverageRating { get; init; }
        public int FiveStarCount { get; init; }
        public int FourStarCount { get; init; }
        public int ThreeStarCount { get; init; }
        public int TwoStarCount { get; init; }
        public int OneStarCount { get; init; }
        public double FiveStarPercentage { get; init; }
        public double FourStarPercentage { get; init; }
        public double ThreeStarPercentage { get; init; }
        public double TwoStarPercentage { get; init; }
        public double OneStarPercentage { get; init; }
    }

    public record ProductReviewsWithStatsDto
    {
        public ProductReviewStatsDto Stats { get; init; } = null!;
        public PaginatedItemsDto<ProductReviewDto> Reviews { get; init; } = null!;
    }
}
