using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Basket.Domain.Repositories;
using Basket.Application.Common.Interfaces;

namespace Basket.Application.Features.Commands.UpdateItemQuantityCommand
{
    public class UpdateItemQuantityCommandHandler : IRequestHandler<UpdateItemQuantityCommand, Unit>
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateItemQuantityCommandHandler(
            IBasketRepository basketRepository, 
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Unit> Handle(UpdateItemQuantityCommand request, CancellationToken cancellationToken)
        {
            var basket = await _basketRepository.GetByIdAsync(request.BasketId);

            if (basket == null)
                throw new KeyNotFoundException($"Basket with ID {request.BasketId} not found");

            basket.UpdateItemQuantity(request.CatalogItemId, request.NewQuantity);
            basket.SetModified("system");

            await _basketRepository.UpdateAsync(basket);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return Unit.Value;
        }
    }
}