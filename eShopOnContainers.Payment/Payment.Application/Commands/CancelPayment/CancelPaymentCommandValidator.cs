using FluentValidation;

namespace Payment.Application.Commands.CancelPayment
{
    public class CancelPaymentCommandValidator : AbstractValidator<CancelPaymentCommand>
    {
        public CancelPaymentCommandValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty().WithMessage("PaymentId is required");
        }
    }
}
