using System;
using Payment.Domain.Common;

namespace Payment.Domain.Events
{
    public class PaymentRefundedEvent : DomainEvent
    {
        public Guid PaymentId { get; }
        public Guid OrderId { get; }
        public decimal RefundAmount { get; }
        public decimal TotalRefundedAmount { get; }
        public decimal OriginalAmount { get; }
        public DateTime RefundedAt { get; }

        public PaymentRefundedEvent(
            Guid paymentId,
            Guid orderId,
            decimal refundAmount,
            decimal totalRefundedAmount,
            decimal originalAmount,
            DateTime refundedAt)
        {
            PaymentId = paymentId;
            OrderId = orderId;
            RefundAmount = refundAmount;
            TotalRefundedAmount = totalRefundedAmount;
            OriginalAmount = originalAmount;
            RefundedAt = refundedAt;
        }
    }
}
