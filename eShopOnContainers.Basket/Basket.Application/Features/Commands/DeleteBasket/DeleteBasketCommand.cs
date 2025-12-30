using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Basket.Application.Features.Commands.DeleteBasket
{
    public record DeleteBasketCommand : IRequest<Unit>
    {
        public Guid Id { get; init; }

        public DeleteBasketCommand(Guid id)
        {
            Id = id;
        }
    }
}