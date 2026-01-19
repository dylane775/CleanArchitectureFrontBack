using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Domain.Repositories;
using Notification.Infrastructure.Data;
using Notification.Infrastructure.Data.Repositories;

namespace Notification.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<NotificationContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("NotificationConnection"),
                    b => b.MigrationsAssembly(typeof(NotificationContext).Assembly.FullName)));

            services.AddScoped<INotificationRepository, NotificationRepository>();

            return services;
        }
    }
}
