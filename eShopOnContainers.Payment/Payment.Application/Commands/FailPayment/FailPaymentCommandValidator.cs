using FluentValidation;

namespace Payment.Application.Commands.FailPayment
{
    public class FailPaymentCommandValidator : AbstractValidator<FailPaymentCommand>
    {
        public FailPaymentCommandValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty().WithMessage("PaymentId is required");

            RuleFor(x => x.FailureReason)
                .NotEmpty().WithMessage("FailureReason is required")
                .MaximumLength(500).WithMessage("FailureReason must not exceed 500 characters");
        }
    }
}
