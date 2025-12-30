using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
namespace Catalog.Application.Commands.CreateCatalogItem
{
    public class CreateCatalogItemCommandValidator : AbstractValidator<CreateCatalogItemCommand>
    {
        public CreateCatalogItemCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Le nom de l'article du catalogue est obligatoire.")
                .MaximumLength(100).WithMessage("Le nom de l'article du catalogue ne peut pas dépasser 100 caractères.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("La description de l'article du catalogue ne peut pas dépasser 500 caractères.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Le prix de l'article du catalogue doit être supérieur à zéro.");

            RuleFor(x => x.AvailableStock)
                .GreaterThanOrEqualTo(0).WithMessage("Le stock disponible de l'article du catalogue ne peut pas être négatif.");

            RuleFor(x => x.CatalogTypeId)
                .NotEmpty().WithMessage("L'ID du type de catalogue est obligatoire.");

            RuleFor(x => x.CatalogBrandId)
                .NotEmpty().WithMessage("L'ID de la marque du catalogue est obligatoire.");

            RuleFor(x => x.PictureFileName)
                .NotEmpty().WithMessage("Le nom du fichier image est obligatoire.")
                .MaximumLength(255).WithMessage("Le nom du fichier image ne peut pas dépasser 255 caractères.");

            RuleFor(x => x.RestockThreshold)
                .GreaterThanOrEqualTo(0).WithMessage("Le seuil de réapprovisionnement ne peut pas être négatif.");

            RuleFor(x => x.MaxStockThreshold)
                .GreaterThanOrEqualTo(0).WithMessage("Le seuil de stock maximum ne peut pas être négatif.")
                .GreaterThanOrEqualTo(x => x.RestockThreshold)
                .WithMessage("Le seuil de stock maximum doit être supérieur ou égal au seuil de réapprovisionnement.");
        }
    }
}