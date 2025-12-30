using System;
using MediatR;

namespace Ordering.Application.Commands.DeleteOrder
{
    public record DeleteOrderCommand : IRequest<Unit>
    {
        public Guid OrderId { get; init; }
    }
}
