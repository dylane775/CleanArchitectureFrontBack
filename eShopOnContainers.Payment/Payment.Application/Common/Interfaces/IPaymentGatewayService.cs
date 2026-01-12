using System;
using System.Threading.Tasks;

namespace Payment.Application.Common.Interfaces
{
    public interface IPaymentGatewayService
    {
        /// <summary>
        /// Initie un paiement auprès du provider (Monetbil, Stripe, etc.)
        /// </summary>
        Task<PaymentGatewayResponse> InitiatePaymentAsync(
            string paymentReference,
            decimal amount,
            string currency,
            string customerEmail,
            string customerPhone,
            string description,
            string callbackUrl,
            string returnUrl);

        /// <summary>
        /// Vérifie le statut d'un paiement
        /// </summary>
        Task<PaymentGatewayStatusResponse> GetPaymentStatusAsync(string transactionId);

        /// <summary>
        /// Demande un remboursement
        /// </summary>
        Task<PaymentGatewayRefundResponse> RefundPaymentAsync(string transactionId, decimal amount);
    }

    public class PaymentGatewayResponse
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string PaymentUrl { get; set; }
        public string QrCodeUrl { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class PaymentGatewayStatusResponse
    {
        public bool Success { get; set; }
        public string Status { get; set; }  // "pending", "completed", "failed"
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class PaymentGatewayRefundResponse
    {
        public bool Success { get; set; }
        public string RefundId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
