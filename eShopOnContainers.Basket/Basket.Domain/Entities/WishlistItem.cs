using System;
using Basket.Domain.Common;

namespace Basket.Domain.Entities
{
    /// <summary>
    /// Article dans la liste de souhaits d'un utilisateur
    /// </summary>
    public class WishlistItem : Entity
    {
        public string UserId { get; private set; }
        public Guid CatalogItemId { get; private set; }
        public string ProductName { get; private set; }
        public decimal Price { get; private set; }
        public string PictureUrl { get; private set; }
        public string BrandName { get; private set; }
        public string CategoryName { get; private set; }
        public DateTime AddedAt { get; private set; }

        // Constructeur protégé pour EF Core
        protected WishlistItem() { }

        public WishlistItem(
            string userId,
            Guid catalogItemId,
            string productName,
            decimal price,
            string pictureUrl = null,
            string brandName = null,
            string categoryName = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name cannot be empty", nameof(productName));

            UserId = userId;
            CatalogItemId = catalogItemId;
            ProductName = productName;
            Price = price;
            PictureUrl = pictureUrl;
            BrandName = brandName;
            CategoryName = categoryName;
            AddedAt = DateTime.UtcNow;
        }

        public void UpdatePrice(decimal newPrice)
        {
            Price = newPrice;
        }

        public void UpdateProductInfo(string productName, decimal price, string pictureUrl, string brandName, string categoryName)
        {
            ProductName = productName ?? ProductName;
            Price = price;
            PictureUrl = pictureUrl ?? PictureUrl;
            BrandName = brandName ?? BrandName;
            CategoryName = categoryName ?? CategoryName;
        }
    }
}
