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
    /// <summary>
    /// Handler qui intercepte le Domain Event et publie un Integration Event vers RabbitMQ
    /// </summary>
    public class ProductPriceChangedDomainEventHandler : INotificationHandler<ProductPriceChangedDomainEvent>
    {

         private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ProductPriceChangedDomainEventHandler> _logger;
        private readonly ICatalogRepository _catalogRepository;
        public ProductPriceChangedDomainEventHandler(
            IPublishEndpoint publishEndpoint,
            ILogger<ProductPriceChangedDomainEventHandler> logger,
            ICatalogRepository catalogRepository
        )
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _catalogRepository = catalogRepository ?? throw new ArgumentNullException(nameof(catalogRepository));
        }

       public async Task Handle(ProductPriceChangedDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling Domain Event: ProductPriceChanged - ProductId: {ProductId}, OldPrice: {OldPrice}, NewPrice: {NewPrice}",
                domainEvent.ProductId,
                domainEvent.OldPrice,
                domainEvent.NewPrice);

            try
            {
                // Récupérer le nom du produit pour l'event
                var product = await _catalogRepository.GetByIdAsync(domainEvent.ProductId);
                var productName = product?.Name ?? "Unknown Product";

                // Transformer le Domain Event en Integration Event
                var integrationEvent = new ProductPriceChangedIntegrationEvent(
                    productId: domainEvent.ProductId,
                    productName: productName,
                    oldPrice: domainEvent.OldPrice,
                    newPrice: domainEvent.NewPrice
                );

                // Publier vers RabbitMQ via MassTransit
                await _publishEndpoint.Publish(integrationEvent, cancellationToken);

                _logger.LogInformation(
                    "Published Integration Event: ProductPriceChanged - ProductId: {ProductId}",
                    domainEvent.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing ProductPriceChanged Integration Event - ProductId: {ProductId}",
                    domainEvent.ProductId);
                
                // Ne pas propager l'erreur pour ne pas bloquer la transaction principale
                // Les messages RabbitMQ seront retentés automatiquement par MassTransit
            }
        }
    }
}