using System;
using Ordering.Domain.Common;
using Ordering.Domain.Exceptions;

namespace Ordering.Domain.Entities
{
    public class OrderItem : Entity
    {
        public Guid CatalogItemId { get; private set; }
        public Guid OrderId { get; private set; }
        public string ProductName { get; private set; } = string.Empty;
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }
        public string? PictureUrl { get; private set; }
        public decimal Discount { get; private set; }

        // Navigation property
        public Order? Order { get; private set; }

        // Constructeur protégé pour EF Core
        protected OrderItem() { }

        public OrderItem(Guid catalogItemId, string productName, decimal unitPrice, int quantity, string? pictureUrl = null, decimal discount = 0)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new OrderingDomainException("Product name cannot be empty", nameof(productName));

            if (unitPrice < 0)
                throw new OrderingDomainException("Unit price cannot be negative");

            if (quantity <= 0)
                throw new OrderingDomainException("Quantity must be positive");

            if (discount < 0 || discount > 1)
                throw new OrderingDomainException("Discount must be between 0 and 1");

            CatalogItemId = catalogItemId;
            ProductName = productName;
            UnitPrice = unitPrice;
            Quantity = quantity;
            PictureUrl = pictureUrl;
            Discount = discount;
        }

        /// <summary>
        /// Calcule le prix total de la ligne de commande (prix unitaire × quantité - remise)
        /// </summary>
        public decimal GetTotalPrice()
        {
            var subtotal = UnitPrice * Quantity;
            var discountAmount = subtotal * Discount;
            return subtotal - discountAmount;
        }

        /// <summary>
        /// Calcule le montant de la remise appliquée
        /// </summary>
        public decimal GetDiscountAmount()
        {
            return UnitPrice * Quantity * Discount;
        }

        /// <summary>
        /// Calcule le sous-total avant remise
        /// </summary>
        public decimal GetSubtotal()
        {
            return UnitPrice * Quantity;
        }

        /// <summary>
        /// Met à jour la quantité de l'article
        /// </summary>
        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new OrderingDomainException("Quantity must be positive");

            Quantity = newQuantity;
        }

        /// <summary>
        /// Applique une remise à l'article
        /// </summary>
        public void ApplyDiscount(decimal discount)
        {
            if (discount < 0 || discount > 1)
                throw new OrderingDomainException("Discount must be between 0 and 1");

            Discount = discount;
        }

        /// <summary>
        /// Met à jour le prix unitaire (par exemple en cas de changement de prix après création de la commande)
        /// </summary>
        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new OrderingDomainException("Unit price cannot be negative");

            UnitPrice = newPrice;
        }

        /// <summary>
        /// Met à jour les détails de l'article
        /// </summary>
        public void UpdateDetails(string productName, decimal unitPrice, int quantity, string? pictureUrl = null)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new OrderingDomainException("Product name cannot be empty", nameof(productName));

            if (unitPrice < 0)
                throw new OrderingDomainException("Unit price cannot be negative");

            if (quantity <= 0)
                throw new OrderingDomainException("Quantity must be positive");

            ProductName = productName;
            UnitPrice = unitPrice;
            Quantity = quantity;
            PictureUrl = pictureUrl;
        }
    }
}