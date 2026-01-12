using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payment.Domain.Repositories;
using Payment.Application.Common.Interfaces;

namespace Payment.Application.Commands.FailPayment
{
    public class FailPaymentCommandHandler : IRequestHandler<FailPaymentCommand, bool>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public FailPaymentCommandHandler(
            IPaymentRepository paymentRepository,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<bool> Handle(FailPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByIdAsync(request.PaymentId);

            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {request.PaymentId} not found");

            // Marquer le paiement comme échoué
            payment.MarkAsFailed(request.FailureReason);
            payment.SetModified("system"); // TODO: Récupérer l'utilisateur actuel

            await _paymentRepository.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
