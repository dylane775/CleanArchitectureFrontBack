using System;

namespace Payment.Application.DTOs.Output
{
    public class PaymentInitiatedResponseDto
    {
        public Guid PaymentId { get; set; }
        public string PaymentReference { get; set; }
        public string Status { get; set; }

        // Informations pour redirection vers le provider de paiement
        public string PaymentUrl { get; set; }  // URL Monetbil ou autre provider
        public string QrCodeUrl { get; set; }    // Pour afficher un QR code si disponible
    }
}
