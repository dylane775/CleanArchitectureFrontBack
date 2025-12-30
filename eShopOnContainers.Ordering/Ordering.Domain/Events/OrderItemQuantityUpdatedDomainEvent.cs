using System;
using Ordering.Domain.Common;

namespace Ordering.Domain.Events
{
    public class OrderItemQuantityUpdatedDomainEvent : DomainEvent
    {
        public Guid OrderId { get; }
        public Guid CatalogItemId { get; }
        public int OldQuantity { get; }
        public int NewQuantity { get; }

        public OrderItemQuantityUpdatedDomainEvent(Guid orderId, Guid catalogItemId, int oldQuantity, int newQuantity)
        {
            OrderId = orderId;
            CatalogItemId = catalogItemId;
            OldQuantity = oldQuantity;
            NewQuantity = newQuantity;
        }
    }
}
