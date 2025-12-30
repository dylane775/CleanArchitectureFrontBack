using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Basket.Application.Features.Commands.RemoveItemFromBasket
{
    public record RemoveItemFromBasketCommand : IRequest<Unit>
    {
        public Guid BasketId { get; init; }
        public Guid CatalogItemId { get; init; }
    }
}