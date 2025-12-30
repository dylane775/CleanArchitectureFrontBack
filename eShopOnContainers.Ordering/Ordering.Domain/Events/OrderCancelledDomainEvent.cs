using System;
using Ordering.Domain.Common;

namespace Ordering.Domain.Events
{
    public class OrderCancelledDomainEvent : DomainEvent
    {
        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public string Reason { get; }
        public string PreviousStatus { get; }

        public OrderCancelledDomainEvent(Guid orderId, Guid customerId, string previousStatus, string? reason = null)
        {
            OrderId = orderId;
            CustomerId = customerId;
            PreviousStatus = previousStatus;
            Reason = reason ?? string.Empty;
        }
    }
}
