using System;
using Catalog.Domain.Common;

namespace Catalog.Domain.Entities
{
    /// <summary>
    /// Entité représentant un avis client sur un produit
    /// </summary>
    public class ProductReview : Entity
    {
        // ====== PROPRIÉTÉS ======

        /// <summary>
        /// ID du produit concerné par l'avis
        /// </summary>
        public Guid CatalogItemId { get; private set; }

        /// <summary>
        /// ID de l'utilisateur qui a laissé l'avis
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Nom d'affichage de l'utilisateur
        /// </summary>
        public string UserDisplayName { get; private set; }

        /// <summary>
        /// Note de 1 à 5 étoiles
        /// </summary>
        public int Rating { get; private set; }

        /// <summary>
        /// Titre de l'avis
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Commentaire détaillé
        /// </summary>
        public string Comment { get; private set; }

        /// <summary>
        /// Indique si l'avis est vérifié (achat confirmé)
        /// </summary>
        public bool IsVerifiedPurchase { get; private set; }

        /// <summary>
        /// Nombre de personnes ayant trouvé cet avis utile
        /// </summary>
        public int HelpfulCount { get; private set; }

        /// <summary>
        /// Nombre total de votes sur l'utilité de l'avis
        /// </summary>
        public int TotalVotes { get; private set; }

        // Navigation
        public CatalogItem CatalogItem { get; private set; }

        // Constructeur privé pour EF Core
        protected ProductReview() { }

        public ProductReview(
            Guid catalogItemId,
            string userId,
            string userDisplayName,
            int rating,
            string title,
            string comment,
            bool isVerifiedPurchase = false)
        {
            if (catalogItemId == Guid.Empty)
                throw new ArgumentException("CatalogItemId cannot be empty", nameof(catalogItemId));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId cannot be null or empty", nameof(userId));

            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be null or empty", nameof(title));

            CatalogItemId = catalogItemId;
            UserId = userId;
            UserDisplayName = userDisplayName ?? "Anonymous";
            Rating = rating;
            Title = title;
            Comment = comment ?? string.Empty;
            IsVerifiedPurchase = isVerifiedPurchase;
            HelpfulCount = 0;
            TotalVotes = 0;
        }

        // ====== MÉTHODES ======

        /// <summary>
        /// Met à jour le contenu de l'avis
        /// </summary>
        public void UpdateContent(int rating, string title, string comment)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be null or empty", nameof(title));

            Rating = rating;
            Title = title;
            Comment = comment ?? string.Empty;
        }

        /// <summary>
        /// Enregistre un vote sur l'utilité de l'avis
        /// </summary>
        /// <param name="isHelpful">True si l'utilisateur a trouvé l'avis utile</param>
        public void AddVote(bool isHelpful)
        {
            TotalVotes++;
            if (isHelpful)
            {
                HelpfulCount++;
            }
        }

        /// <summary>
        /// Marque l'avis comme achat vérifié
        /// </summary>
        public void MarkAsVerifiedPurchase()
        {
            IsVerifiedPurchase = true;
        }

        /// <summary>
        /// Calcule le pourcentage de votes positifs
        /// </summary>
        public double GetHelpfulPercentage()
        {
            if (TotalVotes == 0) return 0;
            return (double)HelpfulCount / TotalVotes * 100;
        }
    }
}
