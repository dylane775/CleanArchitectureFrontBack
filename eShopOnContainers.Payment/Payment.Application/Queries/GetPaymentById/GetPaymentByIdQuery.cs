using System;
using MediatR;
using Payment.Application.DTOs.Output;

namespace Payment.Application.Queries.GetPaymentById
{
    public class GetPaymentByIdQuery : IRequest<PaymentDto>
    {
        public Guid PaymentId { get; }

        public GetPaymentByIdQuery(Guid paymentId)
        {
            PaymentId = paymentId;
        }
    }
}
