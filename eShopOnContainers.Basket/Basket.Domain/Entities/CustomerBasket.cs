using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basket.Domain.Common;
using Basket.Domain.Events;
using Basket.Domain.Exceptions;

namespace Basket.Domain.Entities
{
    public class CustomerBasket : Entity, IAggregateRoot
    {
        // ====== PROPRIÉTÉS ======
        public string CustomerId { get; private set; }

        // ✅ Navigation property que EF Core peut tracker
        // Utiliser ICollection pour permettre à EF Core de détecter les changements
        public ICollection<BasketItem> Items { get; } = new List<BasketItem>();

        // ====== CONSTRUCTEURS ======
        protected CustomerBasket()
        {
            CustomerId = string.Empty;
        }

        public CustomerBasket(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("CustomerId cannot be null or empty", nameof(customerId));

            CustomerId = customerId;
        }

        // ====== MÉTHODES MÉTIER ======
        public void AddItem(Guid catalogItemId, string productName, decimal unitPrice, int quantity, string pictureUrl = null)
        {
            if (quantity <= 0)
                throw new BasketDomainException("Quantity must be positive");

            if (unitPrice < 0)
                throw new BasketDomainException("Unit price cannot be negative");

            var existingItem = Items.FirstOrDefault(i => i.CatalogItemId == catalogItemId);

            if (existingItem != null)
            {
                existingItem.AddQuantity(quantity);
                existingItem.SetModified("system");
            }
            else
            {
                var newItem = new BasketItem(catalogItemId, productName, unitPrice, quantity, pictureUrl);
                newItem.SetCreated("system");
                Items.Add(newItem);

                AddDomainEvent(new BasketItemAddedDomainEvent(Id, catalogItemId, quantity));
            }
        }

        public void RemoveItem(Guid catalogItemId)
        {
            var item = Items.FirstOrDefault(i => i.CatalogItemId == catalogItemId);

            if (item == null)
                throw new BasketDomainException($"Item {catalogItemId} not found in basket");

            Items.Remove(item);
            AddDomainEvent(new BasketItemRemovedDomainEvent(Id, catalogItemId));
        }

        public void UpdateItemQuantity(Guid catalogItemId, int newQuantity)
        {
            if (newQuantity <= 0)
                throw new BasketDomainException("Quantity must be positive");

            var item = Items.FirstOrDefault(i => i.CatalogItemId == catalogItemId);

            if (item == null)
                throw new BasketDomainException($"Item {catalogItemId} not found in basket");

            item.UpdateQuantity(newQuantity);
            item.SetModified("system");
        }

        public void Clear()
        {
            Items.Clear();
        }

        public decimal GetTotalPrice()
        {
            return Items.Sum(i => i.GetTotalPrice());
        }

        public void Checkout()
        {
            if (!Items.Any())
                throw new BasketDomainException("Cannot checkout an empty basket");

            AddDomainEvent(new BasketCheckoutDomainEvent(Id, CustomerId, GetTotalPrice()));
        }
    }
}