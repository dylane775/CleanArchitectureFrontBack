using System.Collections.Generic;
using MediatR;
using Basket.Application.DTOs;

namespace Basket.Application.Queries.Wishlist
{
    public record GetWishlistQuery(string UserId) : IRequest<IEnumerable<WishlistItemDto>>;

    public record CheckWishlistItemQuery(string UserId, System.Guid CatalogItemId) : IRequest<bool>;

    public record GetWishlistCountQuery(string UserId) : IRequest<int>;
}
