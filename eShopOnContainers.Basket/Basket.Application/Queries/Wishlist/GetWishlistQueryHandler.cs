using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Basket.Domain.Repositories;
using Basket.Application.DTOs;

namespace Basket.Application.Queries.Wishlist
{
    public class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, IEnumerable<WishlistItemDto>>
    {
        private readonly IWishlistRepository _wishlistRepository;

        public GetWishlistQueryHandler(IWishlistRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

        public async Task<IEnumerable<WishlistItemDto>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
        {
            var items = await _wishlistRepository.GetByUserIdAsync(request.UserId);

            return items.Select(item => new WishlistItemDto
            {
                Id = item.Id,
                CatalogItemId = item.CatalogItemId,
                ProductName = item.ProductName,
                Price = item.Price,
                PictureUrl = item.PictureUrl,
                BrandName = item.BrandName,
                CategoryName = item.CategoryName,
                AddedAt = item.AddedAt
            });
        }
    }

    public class CheckWishlistItemQueryHandler : IRequestHandler<CheckWishlistItemQuery, bool>
    {
        private readonly IWishlistRepository _wishlistRepository;

        public CheckWishlistItemQueryHandler(IWishlistRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

        public async Task<bool> Handle(CheckWishlistItemQuery request, CancellationToken cancellationToken)
        {
            return await _wishlistRepository.ExistsAsync(request.UserId, request.CatalogItemId);
        }
    }

    public class GetWishlistCountQueryHandler : IRequestHandler<GetWishlistCountQuery, int>
    {
        private readonly IWishlistRepository _wishlistRepository;

        public GetWishlistCountQueryHandler(IWishlistRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

        public async Task<int> Handle(GetWishlistCountQuery request, CancellationToken cancellationToken)
        {
            return await _wishlistRepository.GetCountByUserIdAsync(request.UserId);
        }
    }
}
