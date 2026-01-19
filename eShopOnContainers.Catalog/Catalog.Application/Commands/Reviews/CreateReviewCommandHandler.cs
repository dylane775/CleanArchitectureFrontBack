using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Catalog.Domain.Repositories;
using Catalog.Domain.Entities;
using Catalog.Application.common.Interfaces;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Commands.Reviews
{
    public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ProductReviewDto>
    {
        private readonly IProductReviewRepository _reviewRepository;
        private readonly ICatalogRepository _catalogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateReviewCommandHandler(
            IProductReviewRepository reviewRepository,
            ICatalogRepository catalogRepository,
            IUnitOfWork unitOfWork)
        {
            _reviewRepository = reviewRepository;
            _catalogRepository = catalogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            // Vérifier que le produit existe
            var product = await _catalogRepository.GetByIdAsync(request.CatalogItemId);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {request.CatalogItemId} not found");
            }

            // Vérifier si l'utilisateur a déjà laissé un avis pour ce produit
            var existingReview = await _reviewRepository.HasUserReviewedProductAsync(request.CatalogItemId, request.UserId);
            if (existingReview)
            {
                throw new InvalidOperationException("User has already reviewed this product");
            }

            // Créer l'avis
            var review = new ProductReview(
                catalogItemId: request.CatalogItemId,
                userId: request.UserId,
                userDisplayName: request.UserDisplayName,
                rating: request.Rating,
                title: request.Title,
                comment: request.Comment,
                isVerifiedPurchase: request.IsVerifiedPurchase
            );

            review.SetCreated(request.UserId);

            await _reviewRepository.AddAsync(review);
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
