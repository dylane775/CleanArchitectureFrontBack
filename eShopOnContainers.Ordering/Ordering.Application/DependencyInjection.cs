using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Common.Behaviors;
using MediatR;

namespace Ordering.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, Assembly? infrastructureAssembly = null)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // MediatR - Scanner Application ET Infrastructure pour les Domain Event Handlers
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(assembly);

                // Scanner aussi Infrastructure pour les Domain Event Handlers
                if (infrastructureAssembly != null)
                {
                    config.RegisterServicesFromAssembly(infrastructureAssembly);
                }
            });

            // FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // AutoMapper
            services.AddAutoMapper(assembly);

            // Pipeline Behaviors (ordre important!)
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

            return services;
        }
    }
}
