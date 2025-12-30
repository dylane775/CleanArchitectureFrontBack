using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Basket.Domain.Repositories;
using Basket.Application.Common.Interfaces;

namespace Basket.Application.Features.Commands.ClearBasket
{
    public class ClearBasketCommandHandler : IRequestHandler<ClearBasketCommand, Unit>
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ClearBasketCommandHandler(
            IBasketRepository basketRepository,
            IUnitOfWork unitOfWork)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Unit> Handle(ClearBasketCommand request, CancellationToken cancellationToken)
        {
            var basket = await _basketRepository.GetByIdAsync(request.BasketId);

            if (basket == null)
                throw new KeyNotFoundException($"Basket with ID {request.BasketId} not found");

            basket.Clear();
            basket.SetModified("system");

            await _basketRepository.UpdateAsync(basket);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return Unit.Value;
        }
    }
}