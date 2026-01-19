using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Catalog.Domain.Repositories;
using Catalog.Application.common.Interfaces;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Commands.Reviews
{
    public class VoteReviewCommandHandler : IRequestHandler<VoteReviewCommand, ProductReviewDto>
    {
        private readonly IProductReviewRepository _reviewRepository;
        private readonly IUnitOfWork _unitOfWork;

        public VoteReviewCommandHandler(
            IProductReviewRepository reviewRepository,
            IUnitOfWork unitOfWork)
        {
            _reviewRepository = reviewRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductReviewDto> Handle(VoteReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _reviewRepository.GetByIdAsync(request.ReviewId);

            if (review == null)
            {
                throw new InvalidOperationException($"Review with ID {request.ReviewId} not found");
            }

            review.AddVote(request.IsHelpful);
            review.SetModified("system");

            await _reviewRepository.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ProductReviewDto
            {
                Id = review.Id,
                CatalogItemId = review.CatalogItemId,
                UserId = review.UserId,
                UserDisplayName = review.UserDisplayName,
                Rating = review.Rating,
                Title = review.Title,
                Comment = review.Comment,
                IsVerifiedPurchase = review.IsVerifiedPurchase,
                HelpfulCount = review.HelpfulCount,
                TotalVotes = review.TotalVotes,
                CreatedAt = review.CreatedAt
            };
        }
    }
}
