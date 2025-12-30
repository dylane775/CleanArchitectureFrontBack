using System.Collections.Generic;
using MediatR;
using Ordering.Application.DTOs.Output;

namespace Ordering.Application.Queries.GetOrdersByStatus
{
    public record GetOrdersByStatusQuery : IRequest<IEnumerable<OrderSummaryDto>>
    {
        public string Status { get; init; }

        public GetOrdersByStatusQuery(string status)
        {
            Status = status;
        }
    }
}
