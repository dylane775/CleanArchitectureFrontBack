using System;
using Ordering.Domain.Common;

namespace Ordering.Domain.Events
{
    public class OrderCreatedDomainEvent : DomainEvent
    {
        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public decimal TotalAmount { get; }
        public string OrderStatus { get; }

        public OrderCreatedDomainEvent(Guid orderId, Guid customerId, decimal totalAmount, string orderStatus)
        {
            OrderId = orderId;
            CustomerId = customerId;
            TotalAmount = totalAmount;
            OrderStatus = orderStatus;
        }
    }
}
