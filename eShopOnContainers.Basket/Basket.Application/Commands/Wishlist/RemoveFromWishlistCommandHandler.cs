using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Basket.Domain.Repositories;
using Basket.Application.Interfaces;
using Basket.Application.Common.Interfaces;

namespace Basket.Application.Commands.Wishlist
{
    public class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, bool>
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveFromWishlistCommandHandler(IWishlistRepository wishlistRepository, IUnitOfWork unitOfWork)
        {
            _wishlistRepository = wishlistRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
        {
            var exists = await _wishlistRepository.ExistsAsync(request.UserId, request.CatalogItemId);

            if (!exists)
                return false;

            await _wishlistRepository.RemoveByUserAndProductAsync(request.UserId, request.CatalogItemId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
