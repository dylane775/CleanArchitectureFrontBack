using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Domain.Events;
using eShopOnContainers.IntegrationEvents;

namespace Ordering.Infrastructure.Messaging.DomainEventHandlers
{
    /// <summary>
    /// Handler qui intercepte le Domain Event OrderCreated et publie un Integration Event vers RabbitMQ
    /// </summary>
    public class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<OrderCreatedDomainEventHandler> _logger;

        public OrderCreatedDomainEventHandler(
            IPublishEndpoint publishEndpoint,
            ILogger<OrderCreatedDomainEventHandler> logger)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(OrderCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling Domain Event: OrderCreated - OrderId: {OrderId}, CustomerId: {CustomerId}",
                domainEvent.OrderId,
                domainEvent.CustomerId);

            try
            {
                // Transformer le Domain Event en Integration Event
                var integrationEvent = new OrderCreatedIntegrationEvent(
                    orderId: domainEvent.OrderId,
                    customerId: domainEvent.CustomerId,
                    totalAmount: domainEvent.TotalAmount,
                    orderStatus: domainEvent.OrderStatus
                );

                // Publier vers RabbitMQ via MassTransit
                await _publishEndpoint.Publish(integrationEvent, cancellationToken);

                _logger.LogInformation(
                    "Published Integration Event: OrderCreated - OrderId: {OrderId}",
                    domainEvent.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing OrderCreated Integration Event - OrderId: {OrderId}",
                    domainEvent.OrderId);

                // Ne pas propager l'erreur pour ne pas bloquer la transaction principale
            }
        }
    }
}
