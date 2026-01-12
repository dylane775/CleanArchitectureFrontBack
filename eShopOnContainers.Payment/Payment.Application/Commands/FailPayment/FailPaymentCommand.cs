using System;
using MediatR;

namespace Payment.Application.Commands.FailPayment
{
    public class FailPaymentCommand : IRequest<bool>
    {
        public Guid PaymentId { get; set; }
        public string FailureReason { get; set; }

        public FailPaymentCommand(Guid paymentId, string failureReason)
        {
            PaymentId = paymentId;
            FailureReason = failureReason;
        }
    }
}
