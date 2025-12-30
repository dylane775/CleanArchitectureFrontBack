using System;
using MediatR;

namespace Ordering.Application.Commands.DeliverOrder
{
    public record DeliverOrderCommand : IRequest<Unit>
    {
        public Guid OrderId { get; init; }
    }
}
