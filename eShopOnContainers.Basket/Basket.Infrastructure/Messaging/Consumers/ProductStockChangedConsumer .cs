using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using eShopOnContainers.IntegrationEvents;
using Basket.Infrastructure.Data;

namespace Basket.Infrastructure.Messaging.Consumers
{

     /// <summary>
    /// Consumer qui reçoit l'event de changement de stock depuis Catalog
    /// Peut notifier l'utilisateur si un produit devient indisponible
    /// </summary>
    public class ProductStockChangedConsumer : IConsumer<ProductStockChangedIntegrationEvent>
    {
        private readonly BasketContext _context;
        private readonly ILogger<ProductStockChangedConsumer> _logger;

        public ProductStockChangedConsumer(
            BasketContext context,
            ILogger<ProductStockChangedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProductStockChangedIntegrationEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Received ProductStockChanged event - ProductId: {ProductId}, NewStock: {NewStock}, IsOutOfStock: {IsOutOfStock}",
                @event.ProductId,
                @event.NewStock,
                @event.IsOutOfStock);

            try
            {
                // Vérifier si des paniers contiennent ce produit
                var basketsWithProduct = await _context.BasketItems
                    .Where(item => item.CatalogItemId == @event.ProductId && !item.IsDeleted)
                    .Select(item => new
                    {
                        item.CustomerBasketId,
                        item.Quantity,
                        item.ProductName
                    })
                    .ToListAsync();

                if (!basketsWithProduct.Any())
                {
                    _logger.LogInformation(
                        "No baskets contain ProductId: {ProductId}",
                        @event.ProductId);
                    return;
                }

                // Si le produit est en rupture de stock
                if (@event.IsOutOfStock)
                {
                    _logger.LogWarning(
                        "Product {ProductId} ({ProductName}) is OUT OF STOCK. Found in {Count} baskets",
                        @event.ProductId,
                        @event.ProductName,
                        basketsWithProduct.Count);

                    // TODO: Implémenter une notification aux utilisateurs
                    // Par exemple : envoyer un email, une notification push, etc.
                    // Pour l'instant, on log juste
                }
                else
                {
                    // Vérifier si certains paniers ont une quantité supérieure au stock disponible
                    var problematicBaskets = basketsWithProduct
                        .Where(b => b.Quantity > @event.NewStock)
                        .ToList();

                    if (problematicBaskets.Any())
                    {
                        _logger.LogWarning(
                            "Product {ProductId} has stock {NewStock}, but {Count} baskets have higher quantities",
                            @event.ProductId,
                            @event.NewStock,
                            problematicBaskets.Count);

                        // TODO: Notifier les utilisateurs concernés
                    }
                }

                _logger.LogInformation(
                    "Processed ProductStockChanged event for ProductId: {ProductId}",
                    @event.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing ProductStockChanged event for ProductId: {ProductId}",
                    @event.ProductId);
                
                throw;
            }
        }
    }
}
