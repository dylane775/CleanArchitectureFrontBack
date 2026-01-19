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
    /// Handler qui intercepte le Domain Event OrderShipped et publie un Integration Event vers RabbitMQ
    /// </summary>
    public class OrderShippedDomainEventHandler : INotificationHandler<OrderShippedDomainEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly INotificationClient _notificationClient;
        private readonly ILogger<OrderShippedDomainEventHandler> _logger;

        public OrderShippedDomainEventHandler(
            IPublishEndpoint publishEndpoint,
            INotificationClient notificationClient,
            ILogger<OrderShippedDomainEventHandler> logger)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _notificationClient = notificationClient ?? throw new ArgumentNullException(nameof(notificationClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(OrderShippedDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling Domain Event: OrderShipped - OrderId: {OrderId}, ShippingAddress: {ShippingAddress}",
                domainEvent.OrderId,
                domainEvent.ShippingAddress);

            try
            {
                // Transformer le Domain Event en Integration Event
                var integrationEvent = new OrderShippedIntegrationEvent(
                    orderId: domainEvent.OrderId,
                    customerId: domainEvent.CustomerId,
                    shippingAddress: domainEvent.ShippingAddress
                );

                // Publier vers RabbitMQ via MassTransit
                await _publishEndpoint.Publish(integrationEvent, cancellationToken);

                _logger.LogInformation(
                    "Published Integration Event: OrderShipped - OrderId: {OrderId}",
                    domainEvent.OrderId);

                // Envoyer notification à l'utilisateur
                await _notificationClient.SendOrderShippedNotificationAsync(
                    domainEvent.CustomerId.ToString(),
                    domainEvent.OrderId,
                    domainEvent.ShippingAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing OrderShipped Integration Event - OrderId: {OrderId}",
                    domainEvent.OrderId);

                // Ne pas propager l'erreur pour ne pas bloquer la transaction principale
            }
        }
    }
}
