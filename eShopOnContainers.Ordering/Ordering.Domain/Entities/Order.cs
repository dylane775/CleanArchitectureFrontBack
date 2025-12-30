using System;
using System.Collections.Generic;
using System.Linq;
using Ordering.Domain.Exceptions;
using Ordering.Domain.Common;
using Ordering.Domain.Enums;
using Ordering.Domain.Events;

namespace Ordering.Domain.Entities
{
    public class Order : Entity, IAggregateRoot
    {
        // ====== PROPRIÉTÉS ======
        public Guid CustomerId { get; private set; }
        public string OrderStatus { get; private set; } = string.Empty;
        public decimal TotalAmount { get; private set; }
        public DateTime OrderDate { get; private set; }
        public DateTime? DeliveryDate { get; private set; }
        public string ShippingAddress { get; private set; } = string.Empty;
        public string BillingAddress { get; private set; } = string.Empty;
        public string PaymentMethod { get; private set; } = string.Empty;
        public string CustomerEmail { get; private set; } = string.Empty;
        public string CustomerPhone { get; private set; } = string.Empty;

        // Navigation property - collection d'items de commande
        public ICollection<OrderItem> Items { get; } = new List<OrderItem>();

        // ====== CONSTRUCTEURS ======
        /// <summary>
        /// Constructeur protégé pour EF Core
        /// </summary>
        protected Order()
        {
            ShippingAddress = string.Empty;
            BillingAddress = string.Empty;
            PaymentMethod = string.Empty;
            CustomerEmail = string.Empty;
            CustomerPhone = string.Empty;
            OrderStatus = string.Empty;
        }

        /// <summary>
        /// Crée une nouvelle commande
        /// </summary>
        public Order(
            Guid customerId,
            string shippingAddress,
            string billingAddress,
            string paymentMethod,
            string customerEmail,
            string? customerPhone = null)
        {
            if (customerId == Guid.Empty)
                throw new OrderingDomainException("Customer ID cannot be empty");

            if (string.IsNullOrWhiteSpace(shippingAddress))
                throw new OrderingDomainException("Shipping address cannot be empty");

            if (string.IsNullOrWhiteSpace(billingAddress))
                throw new OrderingDomainException("Billing address cannot be empty");

            if (string.IsNullOrWhiteSpace(paymentMethod))
                throw new OrderingDomainException("Payment method cannot be empty");

            if (string.IsNullOrWhiteSpace(customerEmail))
                throw new OrderingDomainException("Customer email cannot be empty");

            CustomerId = customerId;
            ShippingAddress = shippingAddress;
            BillingAddress = billingAddress;
            PaymentMethod = paymentMethod;
            CustomerEmail = customerEmail;
            CustomerPhone = customerPhone ?? string.Empty;
            OrderDate = DateTime.UtcNow;
            OrderStatus = Enums.OrderStatus.Initial;
            TotalAmount = 0;

            // Publier l'événement de création de commande
            AddDomainEvent(new OrderCreatedDomainEvent(Id, customerId, TotalAmount, OrderStatus));
        }

        // ====== MÉTHODES MÉTIER ======

        /// <summary>
        /// Ajoute un article à la commande
        /// </summary>
        public void AddItem(Guid catalogItemId, string productName, decimal unitPrice, int quantity, string? pictureUrl = null, decimal discount = 0)
        {
            if (OrderStatus != Enums.OrderStatus.Pending)
                throw new OrderingDomainException($"Cannot add items to an order with status {OrderStatus}");

            if (quantity <= 0)
                throw new OrderingDomainException("Quantity must be positive");

            if (unitPrice < 0)
                throw new OrderingDomainException("Unit price cannot be negative");

            // Vérifier si l'article existe déjà
            var existingItem = Items.FirstOrDefault(i => i.CatalogItemId == catalogItemId);

            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
                existingItem.SetModified("system");
            }
            else
            {
                var newItem = new OrderItem(catalogItemId, productName, unitPrice, quantity, pictureUrl, discount);
                newItem.SetCreated("system");
                Items.Add(newItem);

                // Publier l'événement d'ajout d'article
                AddDomainEvent(new OrderItemAddedDomainEvent(Id, catalogItemId, productName, quantity, unitPrice));
            }

            RecalculateTotal();
        }

        /// <summary>
        /// Supprime un article de la commande
        /// </summary>
        public void RemoveItem(Guid catalogItemId)
        {
            if (OrderStatus != Enums.OrderStatus.Pending)
                throw new OrderingDomainException($"Cannot remove items from an order with status {OrderStatus}");

            var item = Items.FirstOrDefault(i => i.CatalogItemId == catalogItemId);

            if (item == null)
                throw new OrderingDomainException($"Item {catalogItemId} not found in order");

            var productName = item.ProductName;
            Items.Remove(item);

            // Publier l'événement de suppression d'article
            AddDomainEvent(new OrderItemRemovedDomainEvent(Id, catalogItemId, productName));

            RecalculateTotal();
        }

        /// <summary>
        /// Met à jour la quantité d'un article
        /// </summary>
        public void UpdateItemQuantity(Guid catalogItemId, int newQuantity)
        {
            if (OrderStatus != Enums.OrderStatus.Pending)
                throw new OrderingDomainException($"Cannot update items in an order with status {OrderStatus}");

            if (newQuantity <= 0)
                throw new OrderingDomainException("Quantity must be positive");

            var item = Items.FirstOrDefault(i => i.CatalogItemId == catalogItemId);

            if (item == null)
                throw new OrderingDomainException($"Item {catalogItemId} not found in order");

            var oldQuantity = item.Quantity;
            item.UpdateQuantity(newQuantity);
            item.SetModified("system");

            // Publier l'événement de mise à jour de quantité
            AddDomainEvent(new OrderItemQuantityUpdatedDomainEvent(Id, catalogItemId, oldQuantity, newQuantity));

            RecalculateTotal();
        }

        /// <summary>
        /// Applique une remise à un article spécifique
        /// </summary>
        public void ApplyItemDiscount(Guid catalogItemId, decimal discount)
        {
            if (OrderStatus != Enums.OrderStatus.Pending)
                throw new OrderingDomainException($"Cannot apply discount to an order with status {OrderStatus}");

            var item = Items.FirstOrDefault(i => i.CatalogItemId == catalogItemId);

            if (item == null)
                throw new OrderingDomainException($"Item {catalogItemId} not found in order");

            item.ApplyDiscount(discount);
            item.SetModified("system");
            RecalculateTotal();
        }

        /// <summary>
        /// Recalcule le montant total de la commande
        /// </summary>
        private void RecalculateTotal()
        {
            TotalAmount = Items.Sum(i => i.GetTotalPrice());
        }

        /// <summary>
        /// Calcule le montant total de la commande
        /// </summary>
        public decimal GetTotalAmount()
        {
            return Items.Sum(i => i.GetTotalPrice());
        }

        /// <summary>
        /// Obtient le nombre total d'articles dans la commande
        /// </summary>
        public int GetTotalItemCount()
        {
            return Items.Sum(i => i.Quantity);
        }

        /// <summary>
        /// Calcule le montant total des remises
        /// </summary>
        public decimal GetTotalDiscount()
        {
            return Items.Sum(i => i.GetDiscountAmount());
        }

        /// <summary>
        /// Calcule le sous-total avant remises
        /// </summary>
        public decimal GetSubtotal()
        {
            return Items.Sum(i => i.GetSubtotal());
        }

        /// <summary>
        /// Soumet la commande pour traitement
        /// </summary>
        public void Submit()
        {
            if (OrderStatus != Enums.OrderStatus.Pending)
                throw new OrderingDomainException($"Cannot submit an order with status {OrderStatus}");

            if (!Items.Any())
                throw new OrderingDomainException("Cannot submit an empty order");

            var oldStatus = OrderStatus;
            OrderStatus = Enums.OrderStatus.Processing;
            RecalculateTotal();

            // Publier les événements
            AddDomainEvent(new OrderSubmittedDomainEvent(Id, CustomerId, TotalAmount, GetTotalItemCount()));
            AddDomainEvent(new OrderStatusChangedDomainEvent(Id, oldStatus, OrderStatus));
        }

        /// <summary>
        /// Marque la commande comme expédiée
        /// </summary>
        public void MarkAsShipped()
        {
            if (OrderStatus != Enums.OrderStatus.Processing)
                throw new OrderingDomainException($"Cannot ship an order with status {OrderStatus}");

            var oldStatus = OrderStatus;
            OrderStatus = Enums.OrderStatus.Shipped;

            // Publier les événements
            AddDomainEvent(new OrderShippedDomainEvent(Id, CustomerId, ShippingAddress));
            AddDomainEvent(new OrderStatusChangedDomainEvent(Id, oldStatus, OrderStatus));
        }

        /// <summary>
        /// Marque la commande comme livrée
        /// </summary>
        public void MarkAsDelivered()
        {
            if (OrderStatus != Enums.OrderStatus.Shipped)
                throw new OrderingDomainException($"Cannot mark as delivered an order with status {OrderStatus}");

            var oldStatus = OrderStatus;
            OrderStatus = Enums.OrderStatus.Delivered;
            DeliveryDate = DateTime.UtcNow;

            // Publier les événements
            AddDomainEvent(new OrderDeliveredDomainEvent(Id, CustomerId, DeliveryDate.Value));
            AddDomainEvent(new OrderStatusChangedDomainEvent(Id, oldStatus, OrderStatus));
        }

        /// <summary>
        /// Annule la commande
        /// </summary>
        public void Cancel(string? reason = null)
        {
            if (OrderStatus == Enums.OrderStatus.Delivered)
                throw new OrderingDomainException("Cannot cancel a delivered order");

            if (OrderStatus == Enums.OrderStatus.Cancelled)
                throw new OrderingDomainException("Order is already cancelled");

            var oldStatus = OrderStatus;
            OrderStatus = Enums.OrderStatus.Cancelled;

            // Publier les événements
            AddDomainEvent(new OrderCancelledDomainEvent(Id, CustomerId, oldStatus, reason));
            AddDomainEvent(new OrderStatusChangedDomainEvent(Id, oldStatus, OrderStatus));
        }

        /// <summary>
        /// Met à jour l'adresse de livraison
        /// </summary>
        public void UpdateShippingAddress(string newAddress)
        {
            if (OrderStatus == Enums.OrderStatus.Shipped || OrderStatus == Enums.OrderStatus.Delivered)
                throw new OrderingDomainException($"Cannot update shipping address for an order with status {OrderStatus}");

            if (string.IsNullOrWhiteSpace(newAddress))
                throw new OrderingDomainException("Shipping address cannot be empty");

            ShippingAddress = newAddress;
        }

        /// <summary>
        /// Met à jour l'adresse de facturation
        /// </summary>
        public void UpdateBillingAddress(string newAddress)
        {
            if (OrderStatus == Enums.OrderStatus.Delivered)
                throw new OrderingDomainException("Cannot update billing address for a delivered order");

            if (string.IsNullOrWhiteSpace(newAddress))
                throw new OrderingDomainException("Billing address cannot be empty");

            BillingAddress = newAddress;
        }

        /// <summary>
        /// Vérifie si la commande peut être modifiée
        /// </summary>
        public bool CanBeModified()
        {
            return OrderStatus == Enums.OrderStatus.Pending;
        }

        /// <summary>
        /// Vérifie si la commande peut être annulée
        /// </summary>
        public bool CanBeCancelled()
        {
            return OrderStatus != Enums.OrderStatus.Delivered &&
                   OrderStatus != Enums.OrderStatus.Cancelled;
        }
    }
}