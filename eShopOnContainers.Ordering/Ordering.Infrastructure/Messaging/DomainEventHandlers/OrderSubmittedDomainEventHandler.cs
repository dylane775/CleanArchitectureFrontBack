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
    /// Handler qui intercepte le Domain Event OrderSubmitted et publie un Integration Event vers RabbitMQ
    /// </summary>
    public class OrderSubmittedDomainEventHandler : INotificationHandler<OrderSubmittedDomainEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<OrderSubmittedDomainEventHandler> _logger;

        public OrderSubmittedDomainEventHandler(
            IPublishEndpoint publishEndpoint,
            ILogger<OrderSubmittedDomainEventHandler> logger)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(OrderSubmittedDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling Domain Event: OrderSubmitted - OrderId: {OrderId}, TotalItems: {TotalItems}",
                domainEvent.OrderId,
                domainEvent.TotalItems);

            try
            {
                // Transformer le Domain Event en Integration Event
                var integrationEvent = new OrderSubmittedIntegrationEvent(
                    orderId: domainEvent.OrderId,
                    customerId: domainEvent.CustomerId,
                    totalAmount: domainEvent.TotalAmount,
                    itemCount: domainEvent.TotalItems
                );

                // Publier vers RabbitMQ via MassTransit
                await _publishEndpoint.Publish(integrationEvent, cancellationToken);

                _logger.LogInformation(
                    "Published Integration Event: OrderSubmitted - OrderId: {OrderId}",
                    domainEvent.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing OrderSubmitted Integration Event - OrderId: {OrderId}",
                    domainEvent.OrderId);

                // Ne pas propager l'erreur pour ne pas bloquer la transaction principale
            }
        }
    }
}
