using System;
using Payment.Domain.Common;

namespace Payment.Domain.Events
{
    public class PaymentFailedEvent : DomainEvent
    {
        public Guid PaymentId { get; }
        public Guid OrderId { get; }
        public string CustomerId { get; }
        public decimal Amount { get; }
        public string FailureReason { get; }
        public DateTime FailedAt { get; }

        public PaymentFailedEvent(
            Guid paymentId,
            Guid orderId,
            string customerId,
            decimal amount,
            string failureReason,
            DateTime failedAt)
        {
            PaymentId = paymentId;
            OrderId = orderId;
            CustomerId = customerId;
            Amount = amount;
            FailureReason = failureReason;
            FailedAt = failedAt;
        }
    }
}
