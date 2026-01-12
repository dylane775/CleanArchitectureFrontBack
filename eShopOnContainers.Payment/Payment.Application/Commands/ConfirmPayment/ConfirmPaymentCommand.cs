using System;
using MediatR;

namespace Payment.Application.Commands.ConfirmPayment
{
    public class ConfirmPaymentCommand : IRequest<bool>
    {
        public Guid PaymentId { get; set; }
        public string TransactionId { get; set; }

        public ConfirmPaymentCommand(Guid paymentId, string transactionId)
        {
            PaymentId = paymentId;
            TransactionId = transactionId;
        }
    }
}
