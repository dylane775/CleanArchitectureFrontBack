using System;
using System.Collections.Generic;
using MediatR;
using Ordering.Application.DTOs.Output;

namespace Ordering.Application.Queries.GetOrderByCustomerIdAndStatus
{
    public record GetOrdersByCustomerIdAndStatusQuery : IRequest<IEnumerable<OrderSummaryDto>>
    {
        public Guid CustomerId { get; init; }
        public string Status { get; init; }

        public GetOrdersByCustomerIdAndStatusQuery(Guid customerId, string status)
        {
            CustomerId = customerId;
            Status = status;
        }
    }
}