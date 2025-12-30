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
    /// <summary>
    /// Consumer qui reçoit l'event de changement de prix depuis Catalog
    /// Met à jour le prix dans tous les paniers contenant ce produit
    /// </summary>
    public class ProductPriceChangedConsumer : IConsumer<ProductPriceChangedIntegrationEvent>
    {
        private readonly BasketContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductPriceChangedConsumer> _logger;

        public ProductPriceChangedConsumer(
            BasketContext context,
            IUnitOfWork unitOfWork,
            ILogger<ProductPriceChangedConsumer> logger)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

         public async Task Consume(ConsumeContext<ProductPriceChangedIntegrationEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Received ProductPriceChanged event - ProductId: {ProductId}, OldPrice: {OldPrice}, NewPrice: {NewPrice}",
                @event.ProductId,
                @event.OldPrice,
                @event.NewPrice);

            try
            {
                // Trouver tous les BasketItems contenant ce produit
                var basketItems = await _context.BasketItems
                    .Where(item => item.CatalogItemId == @event.ProductId && !item.IsDeleted)
                    .ToListAsync();

                if (!basketItems.Any())
                {
                    _logger.LogInformation(
                        "No basket items found for ProductId: {ProductId}",
                        @event.ProductId);
                    return;
                }

                _logger.LogInformation(
                    "Found {Count} basket items to update for ProductId: {ProductId}",
                    basketItems.Count,
                    @event.ProductId);

                // Mettre à jour le prix dans chaque BasketItem
                foreach (var item in basketItems)
                {
                    item.UpdatePrice(@event.NewPrice);
                    item.SetModified("system"); // Audit trail
                }

                // Sauvegarder les modifications
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully updated {Count} basket items with new price for ProductId: {ProductId}",
                    basketItems.Count,
                    @event.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating basket items for ProductId: {ProductId}",
                    @event.ProductId);
                
                // Rejeter le message pour retry automatique par MassTransit
                throw;
            }
        }
    }
}