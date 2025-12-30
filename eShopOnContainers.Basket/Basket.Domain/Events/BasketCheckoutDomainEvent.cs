using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basket.Domain.Common;

namespace Basket.Domain.Events
{
    public class BasketCheckoutDomainEvent : DomainEvent
    {
         public Guid BasketId { get; }
        public string CustomerId { get; }
        public decimal TotalAmount { get; }

        public BasketCheckoutDomainEvent(Guid basketId, string customerId, decimal totalAmount)
        {
            BasketId = basketId;
            CustomerId = customerId;
            TotalAmount = totalAmount;
        }
    }
}