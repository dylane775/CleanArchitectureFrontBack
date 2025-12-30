using System;
using MediatR;
using Ordering.Application.DTOs.Output;

namespace Ordering.Application.Queries.GetOrderById
{
    public record GetOrderByIdQuery : IRequest<OrderDto>
    {
        public Guid OrderId { get; init; }

        public GetOrderByIdQuery(Guid orderId)
        {
            OrderId = orderId;
        }
    }
}
