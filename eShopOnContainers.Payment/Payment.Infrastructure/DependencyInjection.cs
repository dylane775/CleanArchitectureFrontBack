using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Common.Interfaces;
using Payment.Domain.Repositories;
using Payment.Infrastructure.Data;
using Payment.Infrastructure.Data.Repositories;
using Payment.Infrastructure.PaymentGateways.Monetbil;
using MassTransit;
using System.Reflection;

namespace Payment.Infrastructure
{
    /// <summary>
    /// Configuration de l'injection de dépendances pour la couche Infrastructure
    /// Enregistre tous les services : DbContext, Repositories, UnitOfWork, PaymentGateway, MassTransit
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ====================================
            // 1. DBCONTEXT (Entity Framework Core)
            // ====================================

            services.AddDbContext<PaymentContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("PaymentDb");

                options.UseSqlServer(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(PaymentContext).Assembly.FullName);
                    });

                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // ====================================
            // 2. REPOSITORIES (Implémentations)
            // ====================================

            services.AddScoped<IPaymentRepository, PaymentRepository>();

            // ====================================
            // 3. UNIT OF WORK (Transactions + Events)
            // ====================================

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ====================================
            // 4. PAYMENT GATEWAY (Monetbil)
            // ====================================

            // Configuration Monetbil depuis appsettings.json
            services.Configure<MonetbilSettings>(configuration.GetSection("MonetbilSettings"));

            // HttpClient pour Monetbil
            services.AddHttpClient<IPaymentGatewayService, MonetbilPaymentGateway>();

            // ====================================
            // 5. MASSTRANSIT + RABBITMQ (Messaging)
            // ====================================

            services.AddMassTransit(config =>
            {
                // Enregistrer tous les Consumers automatiquement
                config.AddConsumers(Assembly.GetExecutingAssembly());

                // Configuration RabbitMQ
                config.UsingRabbitMq((context, cfg) =>
                {
                    // Récupérer les paramètres RabbitMQ depuis appsettings.json
                    var rabbitMqHost = configuration["RabbitMQSettings:Host"] ?? "localhost";
                    var rabbitMqUsername = configuration["RabbitMQSettings:Username"] ?? "guest";
                    var rabbitMqPassword = configuration["RabbitMQSettings:Password"] ?? "guest";

                    cfg.Host(rabbitMqHost, h =>
                    {
                        h.Username(rabbitMqUsername);
                        h.Password(rabbitMqPassword);
                    });

                    // Configuration des endpoints (queues)
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
