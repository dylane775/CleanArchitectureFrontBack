using System;
using MediatR;

namespace Basket.Application.Commands.Wishlist
{
    public record RemoveFromWishlistCommand(string UserId, Guid CatalogItemId) : IRequest<bool>;
}
