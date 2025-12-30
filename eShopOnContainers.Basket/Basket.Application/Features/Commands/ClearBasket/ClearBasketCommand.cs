using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Basket.Application.Features.Commands.ClearBasket
{
    public record ClearBasketCommand : IRequest<Unit>
    {
        public Guid BasketId { get; init; }
    }
}