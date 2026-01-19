using System;
using MediatR;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Commands.Reviews
{
    public record CreateReviewCommand : IRequest<ProductReviewDto>
    {
        public Guid CatalogItemId { get; init; }
        public string UserId { get; init; } = string.Empty;
        public string UserDisplayName { get; init; } = string.Empty;
        public int Rating { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Comment { get; init; } = string.Empty;
        public bool IsVerifiedPurchase { get; init; }
    }
}
