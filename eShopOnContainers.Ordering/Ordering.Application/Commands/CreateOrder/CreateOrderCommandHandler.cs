using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Ordering.Domain.Entities;
using Ordering.Domain.Repositories;
using Ordering.Application.Common.Interfaces;

namespace Ordering.Application.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // Créer la commande
            var order = new Order(
                request.CustomerId,
                request.ShippingAddress,
                request.BillingAddress,
                request.PaymentMethod,
                request.CustomerEmail,
                request.CustomerPhone
            );

            order.SetCreated("system"); // TODO: Récupérer l'utilisateur actuel

            // Ajouter les articles
            foreach (var itemDto in request.Items)
            {
                order.AddItem(
                    itemDto.CatalogItemId,
                    itemDto.ProductName,
                    itemDto.UnitPrice,
                    itemDto.Quantity,
                    itemDto.PictureUrl,
                    itemDto.Discount
                );
            }

            // Sauvegarder
            var createdOrder = await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return createdOrder.Id;
        }
    }
}
