using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Payment.Domain.Repositories;
using Payment.Application.DTOs.Output;

namespace Payment.Application.Queries.GetPaymentsByCustomerId
{
    public class GetPaymentsByCustomerIdQueryHandler : IRequestHandler<GetPaymentsByCustomerIdQuery, IEnumerable<PaymentDto>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;

        public GetPaymentsByCustomerIdQueryHandler(
            IPaymentRepository paymentRepository,
            IMapper mapper)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<PaymentDto>> Handle(GetPaymentsByCustomerIdQuery request, CancellationToken cancellationToken)
        {
            var payments = await _paymentRepository.GetByCustomerIdAsync(request.CustomerId);

            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }
    }
}
