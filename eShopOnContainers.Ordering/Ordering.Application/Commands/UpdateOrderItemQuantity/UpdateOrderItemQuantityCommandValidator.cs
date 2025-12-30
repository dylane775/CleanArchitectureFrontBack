using FluentValidation;

namespace Ordering.Application.Commands.UpdateOrderItemQuantity
{
    public class UpdateOrderItemQuantityCommandValidator : AbstractValidator<UpdateOrderItemQuantityCommand>
    {
        public UpdateOrderItemQuantityCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("Order ID is required");

            RuleFor(x => x.CatalogItemId)
                .NotEmpty().WithMessage("Catalog item ID is required");

            RuleFor(x => x.NewQuantity)
                .GreaterThan(0).WithMessage("Quantity must be positive");
        }
    }
}
