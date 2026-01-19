using System;
using MediatR;
using Basket.Application.DTOs;

namespace Basket.Application.Commands.Wishlist
{
    public record AddToWishlistCommand(
        string UserId,
        Guid CatalogItemId,
        string ProductName,
        decimal Price,
        string PictureUrl,
        string BrandName,
        string CategoryName
    ) : IRequest<WishlistItemDto>;
}
