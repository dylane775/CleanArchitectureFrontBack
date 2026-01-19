using System;
using MediatR;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.Reviews
{
    public record GetProductReviewsQuery(
        Guid CatalogItemId,
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<ProductReviewsWithStatsDto>;
}
