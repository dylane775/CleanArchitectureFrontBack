using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Catalog.Application.common.Behaviors;
using MediatR;
using System.Diagnostics;


namespace Catalog.Application
{
    /// <summary>
    /// Configuration de l'injection de dépendances pour la couche Application
    /// Enregistre tous les services nécessaires : MediatR, FluentValidation, AutoMapper, Behaviors
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Méthode d'extension pour enregistrer tous les services de la couche Application
        /// </summary>
        /// <param name="services">La collection de services</param>
        /// <returns>La collection de services enrichie</returns>
        
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Récupérer l'assembly de la couche Application
            var assembly = Assembly.GetExecutingAssembly();

             // ====================================
            // 1. MEDIATR - CQRS (Commands & Queries)
            // ====================================
            
            // Enregistre MediatR et scanne l'assembly pour trouver tous les Handlers
            // Trouve automatiquement :
            // - CreateCatalogItemCommandHandler
            // - UpdateCatalogItemCommandHandler
            // - GetCatalogItemsQueryHandler
            // - etc.
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(assembly);
            });

             // ====================================
            // 2. FLUENT VALIDATION - Validators
            // ====================================
            
            // Enregistre tous les validators présents dans l'assembly
            // Trouve automatiquement :
            // - CreateCatalogItemCommandValidator
            // - UpdateCatalogItemCommandValidator
            // - UpdateStockCommandValidator
            // - etc.
            services.AddValidatorsFromAssembly(assembly);
            
            // ====================================
            // 3. AUTOMAPPER - Mappings
            // ====================================
            
            // Enregistre AutoMapper et scanne l'assembly pour trouver tous les Profiles
            // Trouve automatiquement :
            // - MappingProfile
            services.AddAutoMapper(assembly);
            
            // ====================================
            // 4. MEDIATR BEHAVIORS - Pipeline
            // ====================================
            
            // Les behaviors s'exécutent dans l'ORDRE d'enregistrement
            // IMPORTANT : L'ordre est crucial !
            
            // 4.1 UnhandledExceptionBehavior (EN PREMIER - capture TOUTES les erreurs)
            services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(UnhandledExceptionBehavior<,>)
            );

             // 4.2 LoggingBehavior (log toutes les requêtes)
            services.AddTransient(
                typeof(IPipelineBehavior<,>), 
                typeof(LoggingBehavior<,>)
            );

            // 4.3 ValidationBehavior (valide AVANT d'exécuter le handler)
            services.AddTransient(
                typeof(IPipelineBehavior<,>), 
                typeof(ValidationBehavior<,>)
            );

             // 4.4 PerformanceBehavior (mesure le temps d'exécution)
            services.AddTransient(
                typeof(IPipelineBehavior<,>), 
                typeof(PerformanceBehavior<,>)
            );

            // ❌ 4.5 TransactionBehavior DÉSACTIVÉ
            // Raison : Conflit avec UnitOfWork qui gère déjà les transactions
            // Le TransactionBehavior vide le ChangeTracker après commit, ce qui empêche
            // la publication des Domain Events vers RabbitMQ
            // Solution : Le UnitOfWork gère tout (transactions + publication des events)
            
            // services.AddTransient(
            //     typeof(IPipelineBehavior<,>), 
            //     typeof(TransactionBehavior<,>)
            // );

            return services;
        }
    }
}