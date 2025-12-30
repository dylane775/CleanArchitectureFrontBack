using System;
using MediatR;

namespace Ordering.Application.Commands.SubmitOrder
{
    public record SubmitOrderCommand : IRequest<Unit>
    {
        public Guid OrderId { get; init; }
    }
}
