using FluentValidation;

namespace Payment.Application.Commands.InitiatePayment
{
    public class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
    {
        public InitiatePaymentCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("OrderId is required");

            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("CustomerId is required")
                .MaximumLength(100).WithMessage("CustomerId must not exceed 100 characters");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Length(3).WithMessage("Currency must be a 3-letter code (e.g., XAF, USD)");

            RuleFor(x => x.PaymentProvider)
                .NotEmpty().WithMessage("PaymentProvider is required")
                .Must(provider => provider.Equals("Monetbil", System.StringComparison.OrdinalIgnoreCase) ||
                                 provider.Equals("Stripe", System.StringComparison.OrdinalIgnoreCase) ||
                                 provider.Equals("PayPal", System.StringComparison.OrdinalIgnoreCase) ||
                                 provider.Equals("CashOnDelivery", System.StringComparison.OrdinalIgnoreCase))
                .WithMessage("PaymentProvider must be one of: Monetbil, Stripe, PayPal, CashOnDelivery");

            RuleFor(x => x.CustomerEmail)
                .NotEmpty().WithMessage("CustomerEmail is required")
                .EmailAddress().WithMessage("CustomerEmail must be a valid email address");

            RuleFor(x => x.CustomerPhone)
                .MaximumLength(20).WithMessage("CustomerPhone must not exceed 20 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.CallbackUrl)
                .MaximumLength(500).WithMessage("CallbackUrl must not exceed 500 characters");

            RuleFor(x => x.ReturnUrl)
                .MaximumLength(500).WithMessage("ReturnUrl must not exceed 500 characters");
        }
    }
}
