using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Basket.Domain.Entities;
using Basket.Domain.Repositories;
using Basket.Application.Common.Interfaces;
using System.Runtime.CompilerServices;
using AutoMapper;

namespace Basket.Application.Features.Commands.CreateBasket
{
    public class CreateBasketCommandHandler : IRequestHandler<CreateBasketCommand, Guid>
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public CreateBasketCommandHandler(
            IBasketRepository basketRepository, 
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Guid> Handle(CreateBasketCommand request, CancellationToken cancellationToken)
        {
            var basket = _mapper.Map<CustomerBasket>(request);
            basket.SetCreated("system"); // TODO: Récupérer l'utilisateur actuel

            basket = await _basketRepository.AddAsync(basket);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return basket.Id;
        }
    }
}