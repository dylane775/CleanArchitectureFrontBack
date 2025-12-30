using System;
using MediatR;

namespace Ordering.Application.Commands.RemoveItemFromOrder
{
    public record RemoveItemFromOrderCommand : IRequest<Unit>
    {
        public Guid OrderId { get; init; }
        public Guid CatalogItemId { get; init; }
    }
}
