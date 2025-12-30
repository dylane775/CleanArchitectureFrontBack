using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basket.Domain.Exceptions;
using Basket.Domain.Common;

namespace Basket.Domain.Entities
{
    public class BasketItem : Entity
    {
        public Guid CatalogItemId { get; private set; }
        public string ProductName { get; private set; }
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }
        public string PictureUrl { get; private set; }

        // Foreign key vers CustomerBasket
        public Guid CustomerBasketId { get; private set; }
        public CustomerBasket CustomerBasket { get; private set; }

        // Constructeur protégé pour EF Core
        protected BasketItem() { }

        public BasketItem(Guid catalogItemId, string productName, decimal unitPrice, int quantity, string pictureUrl = null)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name cannot be empty", nameof(productName));

            if (unitPrice < 0)
                throw new BasketDomainException("Unit price cannot be negative");

            if (quantity <= 0)
                throw new BasketDomainException("Quantity must be positive");

            CatalogItemId = catalogItemId;
            ProductName = productName;
            UnitPrice = unitPrice;
            Quantity = quantity;
            PictureUrl = pictureUrl;
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new BasketDomainException("Quantity must be positive");

            Quantity = newQuantity;
        }

        public void AddQuantity(int quantityToAdd)
        {
            if (quantityToAdd <= 0)
                throw new BasketDomainException("Quantity to add must be positive");

            Quantity += quantityToAdd;
        }

        public decimal GetTotalPrice()
        {
            return UnitPrice * Quantity;
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new BasketDomainException("Unit price cannot be negative");

            UnitPrice = newPrice;
        }
    }
}
