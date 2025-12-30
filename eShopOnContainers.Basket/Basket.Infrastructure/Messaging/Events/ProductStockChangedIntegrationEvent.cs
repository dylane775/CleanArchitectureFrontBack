using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopOnContainers.IntegrationEvents
{
   /// <summary>
    /// Integration Event publié quand le stock d'un produit change
    /// Permet au Basket de savoir si un produit est toujours disponible
    /// </summary>
    public record ProductStockChangedIntegrationEvent
    {
        /// <summary>
        /// ID du produit dont le stock a changé
        /// </summary>
        public Guid ProductId { get; init; }

        /// <summary>
        /// Nom du produit (pour logs et debug)
        /// </summary>
        public string ProductName { get; init; } = string.Empty;

        /// <summary>
        /// Ancien stock du produit
        /// </summary>
        public int OldStock { get; init; }

        /// <summary>
        /// Nouveau stock du produit
        /// </summary>
        public int NewStock { get; init; }

        /// <summary>
        /// Indique si le produit est maintenant en rupture de stock
        /// </summary>
        public bool IsOutOfStock { get; init; }

        /// <summary>
        /// Date et heure du changement (UTC)
        /// </summary>
        public DateTime ChangedAt { get; init; }

        public ProductStockChangedIntegrationEvent()
        {
            ChangedAt = DateTime.UtcNow;
        }

        public ProductStockChangedIntegrationEvent(
            Guid productId,
            string productName,
            int oldStock,
            int newStock)
        {
            ProductId = productId;
            ProductName = productName;
            OldStock = oldStock;
            NewStock = newStock;
            IsOutOfStock = newStock <= 0;
            ChangedAt = DateTime.UtcNow;
        }
    }
}