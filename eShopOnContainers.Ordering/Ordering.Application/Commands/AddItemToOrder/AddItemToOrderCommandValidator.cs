using FluentValidation;

namespace Ordering.Application.Commands.AddItemToOrder
{
    public class AddItemToOrderCommandValidator : AbstractValidator<AddItemToOrderCommand>
    {
        public AddItemToOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("Order ID is required");

            RuleFor(x => x.CatalogItemId)
                .NotEmpty().WithMessage("Catalog item ID is required");

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be non-negative");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be positive");

            RuleFor(x => x.Discount)
                .InclusiveBetween(0, 1).WithMessage("Discount must be between 0 and 1");
        }
    }
}
