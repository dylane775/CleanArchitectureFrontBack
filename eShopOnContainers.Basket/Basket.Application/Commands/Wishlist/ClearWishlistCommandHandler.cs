using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Basket.Domain.Repositories;
using Basket.Application.Interfaces;
using Basket.Application.Common.Interfaces;

namespace Basket.Application.Commands.Wishlist
{
    public class ClearWishlistCommandHandler : IRequestHandler<ClearWishlistCommand, bool>
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ClearWishlistCommandHandler(IWishlistRepository wishlistRepository, IUnitOfWork unitOfWork)
        {
            _wishlistRepository = wishlistRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ClearWishlistCommand request, CancellationToken cancellationToken)
        {
            await _wishlistRepository.ClearByUserIdAsync(request.UserId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
