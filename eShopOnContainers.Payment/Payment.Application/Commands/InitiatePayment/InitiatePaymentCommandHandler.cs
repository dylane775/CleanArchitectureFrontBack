using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payment.Domain.Repositories;
using Payment.Domain.Enums;
using Payment.Application.DTOs.Output;
using Payment.Application.Common.Interfaces;

namespace Payment.Application.Commands.InitiatePayment
{
    public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, PaymentInitiatedResponseDto>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IUnitOfWork _unitOfWork;

        public InitiatePaymentCommandHandler(
            IPaymentRepository paymentRepository,
            IPaymentGatewayService paymentGatewayService,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _paymentGatewayService = paymentGatewayService ?? throw new ArgumentNullException(nameof(paymentGatewayService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<PaymentInitiatedResponseDto> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
        {
            // Vérifier si un paiement existe déjà pour cette commande
            var existingPayment = await _paymentRepository.GetByOrderIdAsync(request.OrderId);
            if (existingPayment != null && existingPayment.Status != PaymentStatus.Failed && existingPayment.Status != PaymentStatus.Cancelled)
            {
                throw new InvalidOperationException($"A payment already exists for order {request.OrderId}");
            }

            // Parser le provider
            if (!Enum.TryParse<PaymentProvider>(request.PaymentProvider, true, out var provider))
            {
                throw new ArgumentException($"Invalid payment provider: {request.PaymentProvider}");
            }

            // Créer l'entité Payment
            var payment = Domain.Entities.Payment.Create(
                request.OrderId,
                request.CustomerId,
                request.Amount,
                request.Currency,
                provider,
                request.CustomerEmail,
                request.CustomerPhone,
                request.Description,
                request.CallbackUrl,
                request.ReturnUrl
            );

            payment.SetCreated("system"); // TODO: Récupérer l'utilisateur actuel

            // Sauvegarder en BD
            await _paymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Initier le paiement auprès du provider (Monetbil, etc.)
            var gatewayResponse = await _paymentGatewayService.InitiatePaymentAsync(
                payment.PaymentReference,
                payment.Amount,
                payment.Currency,
                payment.CustomerEmail,
                payment.CustomerPhone,
                payment.Description,
                payment.CallbackUrl,
                payment.ReturnUrl
            );

            if (gatewayResponse.Success)
            {
                // Mettre à jour avec le TransactionId du provider
                payment.StartProcessing(gatewayResponse.TransactionId);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Retourner la réponse
            return new PaymentInitiatedResponseDto
            {
                PaymentId = payment.Id,
                PaymentReference = payment.PaymentReference,
                Status = payment.Status.ToString(),
                PaymentUrl = gatewayResponse.PaymentUrl,
                QrCodeUrl = gatewayResponse.QrCodeUrl
            };
        }
    }
}
