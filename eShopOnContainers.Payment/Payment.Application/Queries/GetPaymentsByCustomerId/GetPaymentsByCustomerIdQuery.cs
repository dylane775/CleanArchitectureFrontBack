using System.Collections.Generic;
using MediatR;
using Payment.Application.DTOs.Output;

namespace Payment.Application.Queries.GetPaymentsByCustomerId
{
    public class GetPaymentsByCustomerIdQuery : IRequest<IEnumerable<PaymentDto>>
    {
        public string CustomerId { get; }

        public GetPaymentsByCustomerIdQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}
