using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Ordering.Domain.Repositories;
using Ordering.Application.DTOs.Output;

namespace Ordering.Application.Queries.GetOrderByCustomerIdAndStatus
{
    public class GetOrdersByCustomerIdAndStatusQueryHandler : IRequestHandler<GetOrdersByCustomerIdAndStatusQuery, IEnumerable<OrderSummaryDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetOrdersByCustomerIdAndStatusQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<OrderSummaryDto>> Handle(GetOrdersByCustomerIdAndStatusQuery request, CancellationToken cancellationToken)
        {
            var orders = await _orderRepository.GetByCustomerIdAndStatusAsync(request.CustomerId, request.Status);

            return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
        }
    }
}