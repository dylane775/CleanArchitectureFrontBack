using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace Basket.Application.Features.Commands.UpdateItemQuantityCommand
{
    public class UpdateItemQuantityCommandValidator : AbstractValidator<UpdateItemQuantityCommand>
    {
        public UpdateItemQuantityCommandValidator()
        {
            RuleFor(x => x.BasketId)
                .NotEmpty().WithMessage("Basket ID is required");

            RuleFor(x => x.CatalogItemId)
                .NotEmpty().WithMessage("Catalog Item ID is required");

            RuleFor(x => x.NewQuantity)
                .GreaterThan(0).WithMessage("New Quantity must be greater than zero");
        }
    }
}