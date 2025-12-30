using System;
using MediatR;

namespace Ordering.Application.Commands.UpdateOrderItemQuantity
{
    public record UpdateOrderItemQuantityCommand : IRequest<Unit>
    {
        public Guid OrderId { get; init; }
        public Guid CatalogItemId { get; init; }
        public int NewQuantity { get; init; }
    }
}
