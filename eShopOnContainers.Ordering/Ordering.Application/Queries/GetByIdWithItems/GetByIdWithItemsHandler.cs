using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Ordering.Domain.Repositories;
using Ordering.Application.DTOs.Output;

namespace Ordering.Application.Queries.GetByIdWithItems
{
    public class GetByIdWithItemsHandler : IRequestHandler<GetByIdWithItems, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetByIdWithItemsHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<OrderDto> Handle(GetByIdWithItems request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdWithItemsAsync(request.Id);

            if (order == null)
                throw new KeyNotFoundException($"Order with ID {request.Id} not found");

            return _mapper.Map<OrderDto>(order);
        }
    }
}