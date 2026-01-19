using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace Notification.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNotificationApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
