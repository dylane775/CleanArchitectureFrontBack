using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MediatR;

namespace Basket.Application.Features.Commands.CreateBasket
{
    public record CreateBasketCommand : IRequest<Guid>
    {
        public string CustomerId { get; init; } = string.Empty;
        
    }
}