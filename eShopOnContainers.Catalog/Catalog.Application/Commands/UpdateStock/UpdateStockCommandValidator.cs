using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace Catalog.Application.Commands.UpdateStock
{
    public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
    {
        public UpdateStockCommandValidator()
        {
            RuleFor(x => x.CatalogItemId)
                .NotEmpty().WithMessage("L'ID de l'article du catalogue est obligatoire.")
                .NotEqual(Guid.Empty).WithMessage("L'ID de l'article du catalogue ne peut pas être vide.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("La quantité doit être supérieure à zéro.");
        }
    }
}
