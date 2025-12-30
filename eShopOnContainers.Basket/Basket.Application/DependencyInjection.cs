using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Basket.Application.Common.Behaviors;
using MediatR;
using AutoMapper;

namespace Basket.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // MediatR
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(assembly);
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