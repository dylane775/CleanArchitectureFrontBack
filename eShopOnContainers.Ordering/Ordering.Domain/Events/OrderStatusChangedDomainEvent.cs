using System;
using Ordering.Domain.Common;

namespace Ordering.Domain.Events
{
    public class OrderStatusChangedDomainEvent : DomainEvent
    {
        public Guid OrderId { get; }
        public string OldStatus { get; }
        public string NewStatus { get; }
        public DateTime ChangedAt { get; }

        public OrderStatusChangedDomainEvent(Guid orderId, string oldStatus, string newStatus)
        {
            OrderId = orderId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            ChangedAt = DateTime.UtcNow;
        }
    }
}
