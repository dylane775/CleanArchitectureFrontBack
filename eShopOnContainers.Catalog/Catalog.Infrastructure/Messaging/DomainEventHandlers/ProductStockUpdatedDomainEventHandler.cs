using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;
using Catalog.Domain.Events;
using eShopOnContainers.IntegrationEvents;
using Catalog.Domain.Repositories;

namespace Catalog.Infrastructure.Messaging.DomainEventHandlers
{
    public class ProductStockUpdatedDomainEventHandler : INotificationHandler<ProductStockUpdatedDomainEvent>
    {
         private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ProductStockUpdatedDomainEventHandler> _logger;
        private readonly ICatalogRepository _catalogRepository;
        public ProductStockUpdatedDomainEventHandler(
            IPublishEndpoint publishEndpoint,
            ILogger<ProductStockUpdatedDomainEventHandler> logger,
            ICatalogRepository catalogRepository
        )
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _catalogRepository = catalogRepository ?? throw new ArgumentNullException(nameof(catalogRepository));
        }

        public async Task Handle(ProductStockUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling Domain Event: ProductStockUpdated - ProductId: {ProductId}, OldStock: {OldStock}, NewStock: {NewStock}",
                domainEvent.ProductId,
                domainEvent.OldStock,
                domainEvent.NewStock);

            try
            {
                // Récupérer le nom du produit
                var product = await _catalogRepository.GetByIdAsync(domainEvent.ProductId);
                var productName = product?.Name ?? "Unknown Product";

                // Transformer le Domain Event en Integration Event
                var integrationEvent = new ProductStockChangedIntegrationEvent(
                    productId: domainEvent.ProductId,
                    productName: productName,
                    oldStock: domainEvent.OldStock,
                    newStock: domainEvent.NewStock
                );

                // Publier vers RabbitMQ
                await _publishEndpoint.Publish(integrationEvent, cancellationToken);

                _logger.LogInformation(
                    "Published Integration Event: ProductStockChanged - ProductId: {ProductId}, NewStock: {NewStock}",
                    domainEvent.ProductId,
                    domainEvent.NewStock);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing ProductStockChanged Integration Event - ProductId: {ProductId}",
                    domainEvent.ProductId);
            }
        }
    }
}