using System;

namespace Catalog.Application.DTOs.Input
{
    public record CreateReviewDto
    {
        public Guid CatalogItemId { get; init; }
        public int Rating { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Comment { get; init; } = string.Empty;
    }

    public record UpdateReviewDto
    {
        public int Rating { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Comment { get; init; } = string.Empty;
    }

    public record VoteReviewDto
    {
        public bool IsHelpful { get; init; }
    }
}
