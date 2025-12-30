using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Catalog.Application.common.Interfaces;

namespace Catalog.Application.common.Behaviors
{
    /// <summary>
    /// Behavior qui entoure automatiquement les Commands dans une transaction
    /// En cas d'erreur, rollback automatique
    /// </summary>
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

        public TransactionBehavior(
            IUnitOfWork unitOfWork,
            ILogger<TransactionBehavior<TRequest, TResponse>> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // Vérifier si c'est une Command (modification) et pas une Query (lecture)
            // On ne veut pas de transaction pour les lectures
            var isCommand = requestName.EndsWith("Command");

            if (!isCommand)
            {
                // Si c'est une Query, pas de transaction
                return await next();
            }

            _logger.LogInformation("Begin transaction for {RequestName}", requestName);

            try
            {
                // Démarrer la transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Exécuter le handler
                var response = await next();

                // Commit la transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Committed transaction for {RequestName}", requestName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in transaction for {RequestName}", requestName);

                // Rollback en cas d'erreur
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogInformation("Rolled back transaction for {RequestName}", requestName);

                throw;
            }
        }
    }
}