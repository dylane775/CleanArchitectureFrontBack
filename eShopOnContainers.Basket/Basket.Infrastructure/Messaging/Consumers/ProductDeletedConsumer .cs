using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using eShopOnContainers.IntegrationEvents;
using Basket.Infrastructure.Data;
using Basket.Application.Common.Interfaces;

namespace Basket.Infrastructure.Messaging.Consumers
{
    public class ProductDeletedConsumer : IConsumer<ProductDeletedIntegrationEvent>
    {
        private readonly BasketContext _context;
        private readonly ILogger<ProductDeletedConsumer> _logger;

        public ProductDeletedConsumer(
            BasketContext context,
            ILogger<ProductDeletedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProductDeletedIntegrationEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Received ProductDeleted event - ProductId: {ProductId}, ProductName: {ProductName}",
                @event.ProductId,
                @event.ProductName);

            try
            {
                // Trouver tous les BasketItems contenant ce produit
                var basketItems = await _context.BasketItems
                    .Include(item => item.CustomerBasket)
                    .Where(item => item.CatalogItemId == @event.ProductId && !item.IsDeleted)
                    .ToListAsync();

                if (!basketItems.Any())
                {
                    _logger.LogInformation(
                        "No basket items found for deleted ProductId: {ProductId}",
                        @event.ProductId);
                    return;
                }

                _logger.LogWarning(
                    "Product {ProductId} ({ProductName}) was deleted. Removing from {Count} baskets",
                    @event.ProductId,
                    @event.ProductName,
                    basketItems.Count);

                // Supprimer les items de tous les paniers
                foreach (var item in basketItems)
                {
                    // Soft delete
                    item.SetDeleted("system");
                    
                    // Mettre à jour le panier parent aussi
                    item.CustomerBasket?.SetModified("system");
                }

                // Sauvegarder les modifications
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully removed deleted product {ProductId} from {Count} baskets",
                    @event.ProductId,
                    basketItems.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error removing deleted product {ProductId} from baskets",
                    @event.ProductId);
                
                throw;
            }
        }
    }
}