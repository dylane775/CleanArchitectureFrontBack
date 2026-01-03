using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Basket.Domain.Repositories;
using Basket.Application.DTOs.Output;

namespace Basket.Application.Features.Query.GetBasketByCustomer
{
    public class GetBasketByCustomerQueryHandler : IRequestHandler<GetBasketByCustomerQuery, BasketDto>
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public GetBasketByCustomerQueryHandler(IBasketRepository basketRepository, IMapper mapper)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<BasketDto> Handle(GetBasketByCustomerQuery request, CancellationToken cancellationToken)
        {
            var basket = await _basketRepository.GetByCustomerIdAsync(request.CustomerId);

            if (basket == null)
                return null;

            return _mapper.Map<BasketDto>(basket);
        }
    }
}