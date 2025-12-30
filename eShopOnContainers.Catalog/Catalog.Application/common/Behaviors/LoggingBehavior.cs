using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;


namespace Catalog.Application.common.Behaviors
{
    /// <summary>
    /// Behavior qui log automatiquement toutes les requêtes
    /// Log le nom de la requête, les paramètres et le temps d'exécution
    /// </summary>
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "Handling {RequestName} - Request: {@Request}",
                requestName,
                request
            );

            try
            {
                // Exécuter le handler
                var response = await next();

                stopwatch.Stop();

                _logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMilliseconds}ms - Response: {@Response}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    response
                );

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "Error handling {RequestName} after {ElapsedMilliseconds}ms - Request: {@Request}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    request
                );

                throw;
            }
        }
    }
}