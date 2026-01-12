using System;
using Payment.Domain.Common;

namespace Payment.Domain.Events
{
    public class PaymentCompletedEvent : DomainEvent
    {
        public Guid PaymentId { get; }
        public Guid OrderId { get; }
        public string CustomerId { get; }
        public decimal Amount { get; }
        public string Currency { get; }
        public string TransactionId { get; }
        public DateTime CompletedAt { get; }

        public PaymentCompletedEvent(
            Guid paymentId,
            Guid orderId,
            string customerId,
            decimal amount,
            string currency,
            string transactionId,
            DateTime completedAt)
        {
            PaymentId = paymentId;
            OrderId = orderId;
            CustomerId = customerId;
            Amount = amount;
            Currency = currency;
            TransactionId = transactionId;
            CompletedAt = completedAt;
        }
    }
}
