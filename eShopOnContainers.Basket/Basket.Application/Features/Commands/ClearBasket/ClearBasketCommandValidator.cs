using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace Basket.Application.Features.Commands.ClearBasket
{
    public class ClearBasketCommandValidator : AbstractValidator<ClearBasketCommand>
    {
        public ClearBasketCommandValidator()
        {
            RuleFor(x => x.BasketId)
                .NotEmpty().WithMessage("Basket ID is required");
        }
    }

}