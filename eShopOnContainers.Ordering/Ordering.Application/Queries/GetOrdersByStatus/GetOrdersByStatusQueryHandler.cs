using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Ordering.Domain.Repositories;
using Ordering.Application.DTOs.Output;

namespace Ordering.Application.Queries.GetOrdersByStatus
{
    public class GetOrdersByStatusQueryHandler : IRequestHandler<GetOrdersByStatusQuery, IEnumerable<OrderSummaryDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetOrdersByStatusQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<OrderSummaryDto>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
        {
            var orders = await _orderRepository.GetByStatusAsync(request.Status);

            return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
        }
    }
}
