using System;
using Payment.Domain.Common;
using Payment.Domain.Enums;

namespace Payment.Domain.Events
{
    public class PaymentInitiatedEvent : DomainEvent
    {
        public Guid PaymentId { get; }
        public Guid OrderId { get; }
        public string CustomerId { get; }
        public decimal Amount { get; }
        public string Currency { get; }
        public PaymentProvider Provider { get; }

        public PaymentInitiatedEvent(
            Guid paymentId,
            Guid orderId,
            string customerId,
            decimal amount,
            string currency,
            PaymentProvider provider)
        {
            PaymentId = paymentId;
            OrderId = orderId;
            CustomerId = customerId;
            Amount = amount;
            Currency = currency;
            Provider = provider;
        }
    }
}
