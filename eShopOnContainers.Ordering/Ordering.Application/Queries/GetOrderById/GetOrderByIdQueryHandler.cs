using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Ordering.Domain.Repositories;
using Ordering.Application.DTOs.Output;

namespace Ordering.Application.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId);

            if (order == null)
                throw new KeyNotFoundException($"Order with ID {request.OrderId} not found");

            return _mapper.Map<OrderDto>(order);
        }
    }
}
