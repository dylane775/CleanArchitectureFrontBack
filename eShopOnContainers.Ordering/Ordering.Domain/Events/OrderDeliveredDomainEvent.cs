using System;
using Ordering.Domain.Common;

namespace Ordering.Domain.Events
{
    public class OrderDeliveredDomainEvent : DomainEvent
    {
        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public DateTime DeliveryDate { get; }

        public OrderDeliveredDomainEvent(Guid orderId, Guid customerId, DateTime deliveryDate)
        {
            OrderId = orderId;
            CustomerId = customerId;
            DeliveryDate = deliveryDate;
        }
    }
}
