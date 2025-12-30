using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Basket.Application.DTOs.Output;

namespace Basket.Application.Features.Query.GetBasket
{
    public record GetBasketQuery : IRequest<BasketDto>
    {
        public Guid BasketId { get; init; }

        public GetBasketQuery(Guid basketId)
        {
            BasketId = basketId;
        }
    }
}