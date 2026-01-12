using System;
using MediatR;

namespace Payment.Application.Commands.RefundPayment
{
    public class RefundPaymentCommand : IRequest<bool>
    {
        public Guid PaymentId { get; set; }
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; }

        public RefundPaymentCommand(Guid paymentId, decimal refundAmount, string reason)
        {
            PaymentId = paymentId;
            RefundAmount = refundAmount;
            Reason = reason;
        }
    }
}
