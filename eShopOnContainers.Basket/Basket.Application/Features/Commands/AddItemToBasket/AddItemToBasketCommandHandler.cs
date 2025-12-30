using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basket.Domain.Repositories;
using Basket.Application.Common.Interfaces;
using AutoMapper;
using MediatR;

namespace Basket.Application.Features.Commands.AddItemToBasket
{
    public class AddItemToBasketCommandHandler : IRequestHandler<AddItemToBasketCommand, Unit>
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public AddItemToBasketCommandHandler(
            IBasketRepository basketRepository, 
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Unit> Handle(AddItemToBasketCommand request, CancellationToken cancellationToken)
        {
            var basket = await _basketRepository.GetByIdAsync(request.BasketId);

            if (basket == null)
                throw new KeyNotFoundException($"Basket with ID {request.BasketId} not found");

            basket.AddItem(
                request.CatalogItemId,
                request.ProductName,
                request.UnitPrice,
                request.Quantity,
                request.PictureUrl);

            basket.SetModified("system");

            await _basketRepository.UpdateAsync(basket);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return Unit.Value;
        }
    }
}