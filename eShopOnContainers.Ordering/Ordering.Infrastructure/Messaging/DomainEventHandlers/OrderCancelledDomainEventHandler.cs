using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Domain.Events;
using Ordering.Infrastructure.Services;
using eShopOnContainers.IntegrationEvents;

namespace Ordering.Infrastructure.Messaging.DomainEventHandlers
{
    /// <summary>
    /// Handler qui intercepte le Domain Event OrderCancelled et publie un Integration Event vers RabbitMQ
    /// </summary>
    public class OrderCancelledDomainEventHandler : INotificationHandler<OrderCancelledDomainEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly INotificationClient _notificationClient;
        private readonly ILogger<OrderCancelledDomainEventHandler> _logger;

        public OrderCancelledDomainEventHandler(
            IPublishEndpoint publishEndpoint,
            INotificationClient notificationClient,
            ILogger<OrderCancelledDomainEventHandler> logger)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _notificationClient = notificationClient ?? throw new ArgumentNullException(nameof(notificationClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(OrderCancelledDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling Domain Event: OrderCancelled - OrderId: {OrderId}, PreviousStatus: {PreviousStatus}",
                domainEvent.OrderId,
                domainEvent.PreviousStatus);

            try
            {
                // Transformer le Domain Event en Integration Event
                var integrationEvent = new OrderCancelledIntegrationEvent(
                    orderId: domainEvent.OrderId,
                    customerId: domainEvent.CustomerId,
                    previousStatus: domainEvent.PreviousStatus,
                    cancellationReason: domainEvent.Reason
                );

                // Publier vers RabbitMQ via MassTransit
                await _publishEndpoint.Publish(integrationEvent, cancellationToken);

                _logger.LogInformation(
                    "Published Integration Event: OrderCancelled - OrderId: {OrderId}",
                    domainEvent.OrderId);

                // Envoyer notification à l'utilisateur
                await _notificationClient.SendOrderCancelledNotificationAsync(
                    domainEvent.CustomerId.ToString(),
                    domainEvent.OrderId,
                    domainEvent.Reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing OrderCancelled Integration Event - OrderId: {OrderId}",
                    domainEvent.OrderId);

                // Ne pas propager l'erreur pour ne pas bloquer la transaction principale
            }
        }
    }
}
