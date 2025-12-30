using System;
using Ordering.Domain.Common;

namespace Ordering.Domain.Events
{
    public class OrderItemRemovedDomainEvent : DomainEvent
    {
        public Guid OrderId { get; }
        public Guid CatalogItemId { get; }
        public string ProductName { get; }

        public OrderItemRemovedDomainEvent(Guid orderId, Guid catalogItemId, string productName)
        {
            OrderId = orderId;
            CatalogItemId = catalogItemId;
            ProductName = productName;
        }
    }
}
