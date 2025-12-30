using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Basket.Application.Features.Commands.AddItemToBasket
{
    public class AddItemToBasketCommand : IRequest<Unit>
    {
        public Guid BasketId { get; init; }
        public Guid CatalogItemId { get; init; }
        public string ProductName { get; init; }
        public decimal UnitPrice { get; init; }
        public int Quantity { get; init; }
        public string PictureUrl { get; init; }
    }
}