using System;

namespace Payment.Application.DTOs.Input
{
    public class RefundPaymentDto
    {
        public Guid PaymentId { get; set; }
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; }
    }
}
