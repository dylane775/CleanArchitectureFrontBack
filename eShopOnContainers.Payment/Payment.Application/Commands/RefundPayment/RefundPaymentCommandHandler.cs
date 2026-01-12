using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payment.Domain.Repositories;
using Payment.Application.Common.Interfaces;

namespace Payment.Application.Commands.RefundPayment
{
    public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, bool>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IUnitOfWork _unitOfWork;

        public RefundPaymentCommandHandler(
            IPaymentRepository paymentRepository,
            IPaymentGatewayService paymentGatewayService,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _paymentGatewayService = paymentGatewayService ?? throw new ArgumentNullException(nameof(paymentGatewayService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<bool> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByIdAsync(request.PaymentId);

            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {request.PaymentId} not found");

            // Demander le remboursement au provider
            var refundResponse = await _paymentGatewayService.RefundPaymentAsync(
                payment.TransactionId,
                request.RefundAmount
            );

            if (!refundResponse.Success)
                throw new InvalidOperationException($"Refund failed: {refundResponse.ErrorMessage}");

            // Marquer le paiement comme remboursé
            payment.Refund(request.RefundAmount);
            payment.SetModified("system"); // TODO: Récupérer l'utilisateur actuel

            await _paymentRepository.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
