using MediatR;
using Payment.Application.DTOs.Output;

namespace Payment.Application.Queries.GetPaymentByReference
{
    public class GetPaymentByReferenceQuery : IRequest<PaymentDto>
    {
        public string PaymentReference { get; }

        public GetPaymentByReferenceQuery(string paymentReference)
        {
            PaymentReference = paymentReference;
        }
    }
}
