using System;
using System.Collections.Generic;
using MediatR;
using Ordering.Application.DTOs.Output;

namespace Ordering.Application.Queries.GetOrdersByCustomerId
{
    public record GetOrdersByCustomerIdQuery : IRequest<IEnumerable<OrderSummaryDto>>
    {
        public Guid CustomerId { get; init; }

        public GetOrdersByCustomerIdQuery(Guid customerId)
        {
            CustomerId = customerId;
        }
    }
}
