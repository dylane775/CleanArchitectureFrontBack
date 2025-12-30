using FluentValidation;

namespace Ordering.Application.Commands.RemoveItemFromOrder
{
    public class RemoveItemFromOrderCommandValidator : AbstractValidator<RemoveItemFromOrderCommand>
    {
        public RemoveItemFromOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("Order ID is required");

            RuleFor(x => x.CatalogItemId)
                .NotEmpty().WithMessage("Catalog item ID is required");
        }
    }
}
