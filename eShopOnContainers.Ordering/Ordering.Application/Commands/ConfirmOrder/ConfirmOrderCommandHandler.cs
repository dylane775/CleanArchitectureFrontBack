using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Ordering.Domain.Repositories;
using Ordering.Application.Common.Interfaces;
using Ordering.Domain.Exceptions;

namespace Ordering.Application.Commands.ConfirmOrder
{
    /// <summary>
    /// Handler pour confirmer une commande (après paiement réussi)
    /// </summary>
    public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, Unit>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ConfirmOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Unit> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);

            if (order == null)
                throw new OrderingDomainException($"Order {request.OrderId} not found");

            // Soumettre la commande (Pending → Processing)
            order.Submit();
            order.SetModified("system");

            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
