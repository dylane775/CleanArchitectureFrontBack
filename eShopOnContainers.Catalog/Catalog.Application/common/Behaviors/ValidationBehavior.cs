using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;

namespace Catalog.Application.common.Behaviors
{
   /// <summary>
    /// Behavior qui valide automatiquement toutes les requêtes (Commands/Queries)
    /// S'exécute AVANT le Handler
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Si aucun validator n'est enregistré pour ce type de requête, passer au suivant
            if (!_validators.Any())
            {
                return await next();
            }

            // Créer le contexte de validation
            var context = new ValidationContext<TRequest>(request);

            // Exécuter tous les validators en parallèle
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            // Récupérer toutes les erreurs de validation
            var failures = validationResults
                .SelectMany(result => result.Errors)
                .Where(failure => failure != null)
                .ToList();

            // Si des erreurs existent, lever une ValidationException
            if (failures.Any())
            {
                throw new ValidationException(failures);
            }

            // Si tout est valide, passer au handler suivant
            return await next();
        }
    }
}