using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Basket.Domain.Repositories;
using Basket.Application.DTOs.Output;

namespace Basket.Application.Features.Query.GetBasket
{
    public class GetBasketQueryHandler : IRequestHandler<GetBasketQuery, BasketDto>
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public GetBasketQueryHandler(IBasketRepository basketRepository, IMapper mapper)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<BasketDto> Handle(GetBasketQuery request, CancellationToken cancellationToken)
        {
            var basket = await _basketRepository.GetByIdAsync(request.BasketId);

            if (basket == null)
                throw new KeyNotFoundException($"Basket with ID {request.BasketId} not found");

            return _mapper.Map<BasketDto>(basket);
        }
    }
}