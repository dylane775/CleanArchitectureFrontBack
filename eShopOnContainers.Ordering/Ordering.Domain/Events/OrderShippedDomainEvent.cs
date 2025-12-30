using System;
using Ordering.Domain.Common;

namespace Ordering.Domain.Events
{
    public class OrderShippedDomainEvent : DomainEvent
    {
        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public string ShippingAddress { get; }
        public DateTime ShippedDate { get; }

        public OrderShippedDomainEvent(Guid orderId, Guid customerId, string shippingAddress)
        {
            OrderId = orderId;
            CustomerId = customerId;
            ShippingAddress = shippingAddress;
            ShippedDate = DateTime.UtcNow;
        }
    }
}
