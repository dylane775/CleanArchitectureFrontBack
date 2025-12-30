using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopOnContainers.IntegrationEvents
{

    /// <summary>
    /// Integration Event publié quand un produit est supprimé (soft delete)
    /// Le Basket doit supprimer ce produit de tous les paniers
    /// </summary>
    public record ProductDeletedIntegrationEvent
    {
        /// <summary>
        /// ID du produit supprimé
        /// </summary>
        public Guid ProductId { get; init; }

        /// <summary>
        /// Nom du produit (pour logs et debug)
        /// </summary>
        public string ProductName { get; init; } = string.Empty;

        /// <summary>
        /// Date et heure de la suppression (UTC)
        /// </summary>
        public DateTime DeletedAt { get; init; }

         /// <summary>
        /// Raison de la suppression (optionnel)
        /// </summary>
        public string? Reason { get; init; }

        public ProductDeletedIntegrationEvent()
        {
            DeletedAt = DateTime.UtcNow;
        }

        public ProductDeletedIntegrationEvent(
            Guid productId,
            string productName,
            string? reason = null)
        {
            ProductId = productId;
            ProductName = productName;
            DeletedAt = DateTime.UtcNow;
            Reason = reason;
        }
    }
}