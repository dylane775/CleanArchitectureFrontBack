using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Identity.Application.Common.Behaviors;

namespace Identity.Application
{
    /// <summary>
    /// Dependency injection configuration for Identity.Application layer
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds application layer services to the dependency injection container
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="infrastructureAssembly">Optional infrastructure assembly for Domain Event Handlers</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services, Assembly? infrastructureAssembly = null)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // MediatR - Scan Application and Infrastructure assemblies for handlers
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(assembly);

                // Also scan Infrastructure assembly for Domain Event Handlers
                if (infrastructureAssembly != null)
                {
                    config.RegisterServicesFromAssembly(infrastructureAssembly);
                }
            });

            // FluentValidation - Scan and register all validators
            services.AddValidatorsFromAssembly(assembly);

            // AutoMapper - Scan and register all mapping profiles
            services.AddAutoMapper(assembly);

            // Pipeline Behaviors (order is important!)
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

            return services;
        }
    }
}
