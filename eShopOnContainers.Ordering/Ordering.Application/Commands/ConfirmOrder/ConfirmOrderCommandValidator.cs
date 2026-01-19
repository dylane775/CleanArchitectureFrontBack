using FluentValidation;

namespace Ordering.Application.Commands.ConfirmOrder
{
    /// <summary>
    /// Validateur pour la commande ConfirmOrder
    /// </summary>
    public class ConfirmOrderCommandValidator : AbstractValidator<ConfirmOrderCommand>
    {
        public ConfirmOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithMessage("OrderId is required");
        }
    }
}
