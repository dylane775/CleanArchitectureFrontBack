using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Payment.Domain.Repositories;
using Payment.Application.DTOs.Output;

namespace Payment.Application.Queries.GetPaymentByReference
{
    public class GetPaymentByReferenceQueryHandler : IRequestHandler<GetPaymentByReferenceQuery, PaymentDto>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;

        public GetPaymentByReferenceQueryHandler(
            IPaymentRepository paymentRepository,
            IMapper mapper)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PaymentDto> Handle(GetPaymentByReferenceQuery request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByPaymentReferenceAsync(request.PaymentReference);

            if (payment == null)
                throw new KeyNotFoundException($"Payment with reference {request.PaymentReference} not found");

            return _mapper.Map<PaymentDto>(payment);
        }
    }
}
