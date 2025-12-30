using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Common.Interfaces;

namespace Ordering.Application.Common.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public TransactionBehavior(
            ILogger<TransactionBehavior<TRequest, TResponse>> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // Ne pas utiliser de transaction pour les Queries (lecture seule)
            if (requestName.EndsWith("Query"))
            {
                return await next();
            }

            _logger.LogInformation("Executing command {RequestName} with transaction", requestName);

            try
            {
                // Exécuter le handler (qui fait les modifications en mémoire)
                var response = await next();

                // Sauvegarder UNE SEULE FOIS après l'exécution
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Command {RequestName} executed successfully", requestName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing command {RequestName}", requestName);
                throw;
            }
        }
    }
}
