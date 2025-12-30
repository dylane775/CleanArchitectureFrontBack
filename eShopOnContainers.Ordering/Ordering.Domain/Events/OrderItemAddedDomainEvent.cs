using System;
using Ordering.Domain.Common;

namespace Ordering.Domain.Events
{
    public class OrderItemAddedDomainEvent : DomainEvent
    {
        public Guid OrderId { get; }
        public Guid CatalogItemId { get; }
        public string ProductName { get; }
        public int Quantity { get; }
        public decimal UnitPrice { get; }

        public OrderItemAddedDomainEvent(Guid orderId, Guid catalogItemId, string productName, int quantity, decimal unitPrice)
        {
            OrderId = orderId;
            CatalogItemId = catalogItemId;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
    }
}
