using System;

namespace Payment.Application.DTOs.Input
{
    public class ConfirmPaymentDto
    {
        public Guid PaymentId { get; set; }
        public string TransactionId { get; set; }
    }
}
