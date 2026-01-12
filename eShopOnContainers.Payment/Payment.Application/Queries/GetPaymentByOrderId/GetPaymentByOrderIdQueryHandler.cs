using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Payment.Domain.Repositories;
using Payment.Application.DTOs.Output;

namespace Payment.Application.Queries.GetPaymentByOrderId
{
    public class GetPaymentByOrderIdQueryHandler : IRequestHandler<GetPaymentByOrderIdQuery, PaymentDto>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;

        public GetPaymentByOrderIdQueryHandler(
            IPaymentRepository paymentRepository,
            IMapper mapper)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PaymentDto> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByOrderIdAsync(request.OrderId);

            if (payment == null)
                throw new KeyNotFoundException($"Payment for order {request.OrderId} not found");

            return _mapper.Map<PaymentDto>(payment);
        }
    }
}
