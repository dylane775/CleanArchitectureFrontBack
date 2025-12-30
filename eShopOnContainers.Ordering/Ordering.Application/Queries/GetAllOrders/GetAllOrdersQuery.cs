using System.Collections.Generic;
using MediatR;
using Ordering.Application.DTOs.Output;

namespace Ordering.Application.Queries.GetAllOrders
{
    public record GetAllOrdersQuery : IRequest<IEnumerable<OrderSummaryDto>>
    {
    }
}
