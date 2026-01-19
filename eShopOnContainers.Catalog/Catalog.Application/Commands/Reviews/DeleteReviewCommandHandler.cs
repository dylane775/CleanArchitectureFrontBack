using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Catalog.Domain.Repositories;
using Catalog.Application.common.Interfaces;

namespace Catalog.Application.Commands.Reviews
{
    public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, bool>
    {
        private readonly IProductReviewRepository _reviewRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteReviewCommandHandler(
            IProductReviewRepository reviewRepository,
            IUnitOfWork unitOfWork)
        {
            _reviewRepository = reviewRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _reviewRepository.GetByIdAsync(request.ReviewId);

            if (review == null)
            {
                return false;
            }

            // Vérifier que l'utilisateur est le propriétaire de l'avis
            if (review.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("You can only delete your own reviews");
            }

            await _reviewRepository.DeleteAsync(request.ReviewId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
