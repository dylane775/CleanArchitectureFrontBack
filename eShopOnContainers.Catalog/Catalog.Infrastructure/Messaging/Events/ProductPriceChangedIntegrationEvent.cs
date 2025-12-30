using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopOnContainers.IntegrationEvents
{
    // <summary>
    /// Integration Event publié quand le prix d'un produit change
    /// Ce contrat sera consommé par le service Basket pour mettre à jour les paniers
    /// </summary>
    public record ProductPriceChangedIntegrationEvent
    {
        /// <summary>
        /// ID du produit dont le prix a changé
        /// </summary>
        public Guid ProductId { get; init; }

        /// <summary>
        /// Nom du produit (pour logs et debug)
        /// </summary>
        public string ProductName { get; init; } = string.Empty;

        /// <summary>
        /// Ancien prix du produit
        /// </summary>
        public decimal OldPrice { get; init; }

        /// <summary>
        /// Nouveau prix du produit
        /// </summary>
        public decimal NewPrice { get; init; }

        /// <summary>
        /// Date et heure du changement (UTC)
        /// </summary>
        public DateTime ChangedAt { get; init; }

        public ProductPriceChangedIntegrationEvent()
        {
            ChangedAt = DateTime.UtcNow;
        }

        public ProductPriceChangedIntegrationEvent(
            Guid productId,
            string productName,
            decimal oldPrice,
            decimal newPrice)
        {
            ProductId = productId;
            ProductName = productName;
            OldPrice = oldPrice;
            NewPrice = newPrice;
            ChangedAt = DateTime.UtcNow;
        }
    }
}
