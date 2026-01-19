using MediatR;

namespace Basket.Application.Commands.Wishlist
{
    public record ClearWishlistCommand(string UserId) : IRequest<bool>;
}
