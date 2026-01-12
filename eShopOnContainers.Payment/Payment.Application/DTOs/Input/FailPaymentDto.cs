using System;

namespace Payment.Application.DTOs.Input
{
    public class FailPaymentDto
    {
        public Guid PaymentId { get; set; }
        public string FailureReason { get; set; }
    }
}
