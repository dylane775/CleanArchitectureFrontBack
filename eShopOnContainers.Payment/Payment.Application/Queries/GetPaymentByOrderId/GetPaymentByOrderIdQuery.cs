using System;
using MediatR;
using Payment.Application.DTOs.Output;

namespace Payment.Application.Queries.GetPaymentByOrderId
{
    public class GetPaymentByOrderIdQuery : IRequest<PaymentDto>
    {
        public Guid OrderId { get; }

        public GetPaymentByOrderIdQuery(Guid orderId)
        {
            OrderId = orderId;
        }
    }
}
