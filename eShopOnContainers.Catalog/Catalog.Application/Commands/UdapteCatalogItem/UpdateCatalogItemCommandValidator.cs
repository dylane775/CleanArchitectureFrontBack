using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace Catalog.Application.Commands.UdapteCatalogItem
{
    public class UpdateCatalogItemCommandValidator : AbstractValidator<UpdateCatalogItemCommand>
    {
        public UpdateCatalogItemCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("L'ID du produit est requis");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Le nom du produit est requis")
                .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("La description ne peut pas dépasser 500 caractères");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Le prix doit être supérieur à 0");
        }
    }
}