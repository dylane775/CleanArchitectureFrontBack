using System;
using MediatR;

namespace Payment.Application.Commands.CancelPayment
{
    public class CancelPaymentCommand : IRequest<bool>
    {
        public Guid PaymentId { get; set; }

        public CancelPaymentCommand(Guid paymentId)
        {
            PaymentId = paymentId;
        }
    }
}
