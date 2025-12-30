using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace Basket.Application.Features.Commands.AddItemToBasket
{
    public class AddItemToBasketCommandValidator : AbstractValidator<AddItemToBasketCommand>
    {
        public AddItemToBasketCommandValidator()
        {
            RuleFor(x => x.BasketId)
                .NotEmpty().WithMessage("Basket ID is required");

            RuleFor(x => x.CatalogItemId)
                .NotEmpty().WithMessage("Catalog Item ID is required");

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product Name is required")
                .MaximumLength(200).WithMessage("Product Name cannot exceed 200 characters");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("Unit Price must be greater than zero");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero");
        }
    }
}