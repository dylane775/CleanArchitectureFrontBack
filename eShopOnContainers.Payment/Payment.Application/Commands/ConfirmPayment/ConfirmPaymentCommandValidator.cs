using FluentValidation;

namespace Payment.Application.Commands.ConfirmPayment
{
    public class ConfirmPaymentCommandValidator : AbstractValidator<ConfirmPaymentCommand>
    {
        public ConfirmPaymentCommandValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty().WithMessage("PaymentId is required");

            RuleFor(x => x.TransactionId)
                .MaximumLength(200).WithMessage("TransactionId must not exceed 200 characters");
        }
    }
}
