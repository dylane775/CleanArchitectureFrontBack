using System.Collections.Generic;
using MediatR;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.Reviews
{
    public record GetUserReviewsQuery(string UserId) : IRequest<IEnumerable<ProductReviewDto>>;
}
