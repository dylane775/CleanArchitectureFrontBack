using System;
using MediatR;

namespace Ordering.Application.Commands.CancelOrder
{
    public record CancelOrderCommand : IRequest<Unit>
    {
        public Guid OrderId { get; init; }
        public string? Reason { get; init; }
    }
}
