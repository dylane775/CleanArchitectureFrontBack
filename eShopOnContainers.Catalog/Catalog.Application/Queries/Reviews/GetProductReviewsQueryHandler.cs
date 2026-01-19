using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Catalog.Domain.Repositories;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.Reviews
{
    public class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, ProductReviewsWithStatsDto>
    {
        private readonly IProductReviewRepository _reviewRepository;

        public GetProductReviewsQueryHandler(IProductReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<ProductReviewsWithStatsDto> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
        {
            // Récupérer les statistiques
            var stats = await _reviewRepository.GetStatsByProductIdAsync(request.CatalogItemId);

            // Récupérer les avis paginés
            var pagedReviews = await _reviewRepository.GetPagedByProductIdAsync(
                request.CatalogItemId,
                request.PageIndex,
                request.PageSize
            );

            // Mapper les données
            var reviewDtos = pagedReviews.Data.Select(r => new ProductReviewDto
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
            }).ToList();

            return new ProductReviewsWithStatsDto
            {
                Stats = new ProductReviewStatsDto
                {
                    CatalogItemId = stats.CatalogItemId,
                    TotalReviews = stats.TotalReviews,
                    AverageRating = stats.AverageRating,
                    FiveStarCount = stats.FiveStarCount,
                    FourStarCount = stats.FourStarCount,
                    ThreeStarCount = stats.ThreeStarCount,
                    TwoStarCount = stats.TwoStarCount,
                    OneStarCount = stats.OneStarCount,
                    FiveStarPercentage = stats.FiveStarPercentage,
                    FourStarPercentage = stats.FourStarPercentage,
                    ThreeStarPercentage = stats.ThreeStarPercentage,
                    TwoStarPercentage = stats.TwoStarPercentage,
                    OneStarPercentage = stats.OneStarPercentage
                },
                Reviews = new PaginatedItemsDto<ProductReviewDto>(
                    pagedReviews.PageIndex,
                    pagedReviews.PageSize,
                    pagedReviews.Count,
                    reviewDtos
                )
            };
        }
    }
}
