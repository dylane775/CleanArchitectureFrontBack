using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Catalog.Domain.Repositories;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.Reviews
{
    public class GetUserReviewsQueryHandler : IRequestHandler<GetUserReviewsQuery, IEnumerable<ProductReviewDto>>
    {
        private readonly IProductReviewRepository _reviewRepository;

        public GetUserReviewsQueryHandler(IProductReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<IEnumerable<ProductReviewDto>> Handle(GetUserReviewsQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _reviewRepository.GetByUserIdAsync(request.UserId);

            return reviews.Select(r => new ProductReviewDto
            {
                Id = r.Id,
                CatalogItemId = r.CatalogItemId,
                UserId = r.UserId,
                UserDisplayName = r.UserDisplayName,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                HelpfulCount = r.HelpfulCount,
                TotalVotes = r.TotalVotes,
                CreatedAt = r.CreatedAt
            });
        }
    }
}
