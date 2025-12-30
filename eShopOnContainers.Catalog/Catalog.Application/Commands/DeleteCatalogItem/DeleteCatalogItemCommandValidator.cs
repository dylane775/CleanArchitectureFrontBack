using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace Catalog.Application.Commands.DeleteCatalogItem
{
    public class DeleteCatalogItemCommandValidator : AbstractValidator<DeleteCatalogItemCommand>
    {
        public DeleteCatalogItemCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("L'ID de l'article du catalogue est obligatoire.")
                .NotEqual(Guid.Empty).WithMessage("L'ID de l'article du catalogue ne peut pas être vide.");
        }
    }
}
