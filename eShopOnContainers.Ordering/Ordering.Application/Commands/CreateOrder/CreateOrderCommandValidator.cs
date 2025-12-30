using FluentValidation;

namespace Ordering.Application.Commands.CreateOrder
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer ID is required");

            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("Shipping address is required")
                .MaximumLength(500).WithMessage("Shipping address cannot exceed 500 characters");

            RuleFor(x => x.BillingAddress)
                .NotEmpty().WithMessage("Billing address is required")
                .MaximumLength(500).WithMessage("Billing address cannot exceed 500 characters");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required")
                .MaximumLength(100).WithMessage("Payment method cannot exceed 100 characters");

            RuleFor(x => x.CustomerEmail)
                .NotEmpty().WithMessage("Customer email is required")
                .EmailAddress().WithMessage("Invalid email address")
                .MaximumLength(200).WithMessage("Email cannot exceed 200 characters");

            RuleFor(x => x.CustomerPhone)
                .MaximumLength(50).WithMessage("Phone number cannot exceed 50 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.CustomerPhone));

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Order must contain at least one item");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.CatalogItemId)
                    .NotEmpty().WithMessage("Catalog item ID is required");

                item.RuleFor(x => x.ProductName)
                    .NotEmpty().WithMessage("Product name is required")
                    .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

                item.RuleFor(x => x.UnitPrice)
                    .GreaterThanOrEqualTo(0).WithMessage("Unit price must be non-negative");

                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be positive");

                item.RuleFor(x => x.Discount)
                    .InclusiveBetween(0, 1).WithMessage("Discount must be between 0 and 1");
            });
        }
    }
}
