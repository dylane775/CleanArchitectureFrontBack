using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace Basket.Application.Features.Commands.RemoveItemFromBasket
{
    public class RemoveItemFromBasketCommandValidator : AbstractValidator<RemoveItemFromBasketCommand>
    {
        public RemoveItemFromBasketCommandValidator()
        {
            RuleFor(x => x.BasketId)
                .NotEmpty().WithMessage("Basket ID is required");

            RuleFor(x => x.CatalogItemId)
                .NotEmpty().WithMessage("Catalog Item ID is required");
        }
    }
}