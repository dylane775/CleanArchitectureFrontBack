using System;
using Ordering.Domain.Common;

namespace Ordering.Domain.Events
{
    public class OrderSubmittedDomainEvent : DomainEvent
    {
        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public decimal TotalAmount { get; }
        public int TotalItems { get; }

        public OrderSubmittedDomainEvent(Guid orderId, Guid customerId, decimal totalAmount, int totalItems)
        {
            OrderId = orderId;
            CustomerId = customerId;
            TotalAmount = totalAmount;
            TotalItems = totalItems;
        }
    }
}
