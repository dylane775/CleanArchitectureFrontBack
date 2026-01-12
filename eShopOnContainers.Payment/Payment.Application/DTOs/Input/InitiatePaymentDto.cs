using System;

namespace Payment.Application.DTOs.Input
{
    public class InitiatePaymentDto
    {
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentProvider { get; set; }  // "Monetbil", "Stripe", etc.
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string Description { get; set; }
        public string CallbackUrl { get; set; }
        public string ReturnUrl { get; set; }
    }
}
