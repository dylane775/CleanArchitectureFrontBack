using System;
using Payment.Domain.Common;
using Payment.Domain.Common.Interfaces;
using Payment.Domain.Enums;
using Payment.Domain.Events;
using Payment.Domain.Exceptions;

namespace Payment.Domain.Entities
{
    public class Payment : Entity, IAggregateRoot
    {
        // ====== PROPRIÉTÉS ======
        public Guid OrderId { get; private set; }
        public string CustomerId { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }
        public PaymentStatus Status { get; private set; }
        public PaymentProvider Provider { get; private set; }

        // Informations de transaction
        public string TransactionId { get; private set; }  // ID de la transaction chez le provider (Monetbil, etc.)
        public string PaymentReference { get; private set; }  // Notre référence interne

        // Métadonnées du paiement
        public string CustomerEmail { get; private set; }
        public string CustomerPhone { get; private set; }
        public string Description { get; private set; }

        // Informations de callback/webhook
        public string CallbackUrl { get; private set; }
        public string ReturnUrl { get; private set; }

        // Informations de résultat
        public string FailureReason { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public DateTime? FailedAt { get; private set; }

        // Pour les remboursements
        public decimal RefundedAmount { get; private set; }
        public DateTime? RefundedAt { get; private set; }

        // ====== CONSTRUCTEUR (pour EF Core) ======
        private Payment() { }

        // ====== FACTORY METHOD ======
        public static Payment Create(
            Guid orderId,
            string customerId,
            decimal amount,
            string currency,
            PaymentProvider provider,
            string customerEmail,
            string customerPhone,
            string description,
            string callbackUrl,
            string returnUrl)
        {
            // Validations
            if (orderId == Guid.Empty)
                throw new PaymentDomainException("OrderId cannot be empty");

            if (string.IsNullOrWhiteSpace(customerId))
                throw new PaymentDomainException("CustomerId is required");

            if (amount <= 0)
                throw new PaymentDomainException("Amount must be greater than zero");

            if (string.IsNullOrWhiteSpace(currency))
                throw new PaymentDomainException("Currency is required");

            if (string.IsNullOrWhiteSpace(customerEmail))
                throw new PaymentDomainException("Customer email is required");

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                CustomerId = customerId,
                Amount = amount,
                Currency = currency,
                Provider = provider,
                Status = PaymentStatus.Pending,
                CustomerEmail = customerEmail,
                CustomerPhone = customerPhone,
                Description = description,
                CallbackUrl = callbackUrl,
                ReturnUrl = returnUrl,
                PaymentReference = GeneratePaymentReference(),
                RefundedAmount = 0
            };

            // Domain Event
            payment.AddDomainEvent(new PaymentInitiatedEvent(
                payment.Id,
                payment.OrderId,
                payment.CustomerId,
                payment.Amount,
                payment.Currency,
                payment.Provider
            ));

            return payment;
        }

        // ====== MÉTHODES MÉTIER ======

        public void StartProcessing(string transactionId)
        {
            if (Status != PaymentStatus.Pending)
                throw new PaymentDomainException($"Cannot start processing payment in {Status} status");

            if (string.IsNullOrWhiteSpace(transactionId))
                throw new PaymentDomainException("TransactionId is required to start processing");

            Status = PaymentStatus.Processing;
            TransactionId = transactionId;
        }

        public void MarkAsCompleted(string transactionId = null)
        {
            if (Status != PaymentStatus.Processing && Status != PaymentStatus.Pending)
                throw new PaymentDomainException($"Cannot complete payment in {Status} status");

            Status = PaymentStatus.Completed;
            CompletedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(transactionId))
                TransactionId = transactionId;

            // Domain Event
            AddDomainEvent(new PaymentCompletedEvent(
                Id,
                OrderId,
                CustomerId,
                Amount,
                Currency,
                TransactionId,
                CompletedAt.Value
            ));
        }

        public void MarkAsFailed(string failureReason)
        {
            if (Status == PaymentStatus.Completed)
                throw new PaymentDomainException("Cannot mark a completed payment as failed");

            if (Status == PaymentStatus.Refunded)
                throw new PaymentDomainException("Cannot mark a refunded payment as failed");

            if (string.IsNullOrWhiteSpace(failureReason))
                throw new PaymentDomainException("Failure reason is required");

            Status = PaymentStatus.Failed;
            FailureReason = failureReason;
            FailedAt = DateTime.UtcNow;

            // Domain Event
            AddDomainEvent(new PaymentFailedEvent(
                Id,
                OrderId,
                CustomerId,
                Amount,
                failureReason,
                FailedAt.Value
            ));
        }

        public void Cancel()
        {
            if (Status == PaymentStatus.Completed)
                throw new PaymentDomainException("Cannot cancel a completed payment. Use Refund instead");

            if (Status == PaymentStatus.Cancelled)
                throw new PaymentDomainException("Payment is already cancelled");

            Status = PaymentStatus.Cancelled;
        }

        public void Refund(decimal refundAmount)
        {
            if (Status != PaymentStatus.Completed)
                throw new PaymentDomainException("Only completed payments can be refunded");

            if (refundAmount <= 0)
                throw new PaymentDomainException("Refund amount must be greater than zero");

            if (refundAmount > (Amount - RefundedAmount))
                throw new PaymentDomainException($"Refund amount cannot exceed remaining amount ({Amount - RefundedAmount})");

            RefundedAmount += refundAmount;
            RefundedAt = DateTime.UtcNow;

            if (RefundedAmount >= Amount)
            {
                Status = PaymentStatus.Refunded;
            }
            else
            {
                Status = PaymentStatus.PartiallyRefunded;
            }

            // Domain Event
            AddDomainEvent(new PaymentRefundedEvent(
                Id,
                OrderId,
                refundAmount,
                RefundedAmount,
                Amount,
                RefundedAt.Value
            ));
        }

        // ====== MÉTHODES UTILITAIRES ======

        private static string GeneratePaymentReference()
        {
            // Format: PAY-YYYYMMDD-XXXXXX (exemple: PAY-20260112-A1B2C3)
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"PAY-{datePart}-{randomPart}";
        }

        public bool IsCompleted() => Status == PaymentStatus.Completed;
        public bool IsFailed() => Status == PaymentStatus.Failed;
        public bool IsPending() => Status == PaymentStatus.Pending;
        public bool IsProcessing() => Status == PaymentStatus.Processing;
        public bool CanBeRefunded() => Status == PaymentStatus.Completed && RefundedAmount < Amount;
    }
}
