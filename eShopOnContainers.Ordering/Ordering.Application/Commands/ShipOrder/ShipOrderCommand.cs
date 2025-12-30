using System;
using MediatR;

namespace Ordering.Application.Commands.ShipOrder
{
    public record ShipOrderCommand : IRequest<Unit>
    {
        public Guid OrderId { get; init; }
    }
}
