using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Basket.Application.Common.Interfaces;
using Basket.Domain.Repositories;
using Basket.Infrastructure.Data;
using Basket.Infrastructure.Data.Repositories;
using MassTransit;
using System.Reflection;

namespace Basket.Infrastructure
{
     /// <summary>
    /// Configuration de l'injection de dépendances pour la couche Infrastructure
    /// Enregistre tous les services : DbContext, Repositories, UnitOfWork, MassTransit
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
            
            services.AddDbContext<BasketContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("BasketDb");
                
                options.UseSqlServer(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(BasketContext).Assembly.FullName);
                    });

                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // ====================================
            // 2. REPOSITORIES (Implémentations)
            // ====================================
            
            services.AddScoped<IBasketRepository, BasketRepository>();

            // ====================================
            // 3. UNIT OF WORK (Transactions + Events)
            // ====================================
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ====================================
            // 4. MASSTRANSIT + RABBITMQ (Messaging)
            // ====================================
            
            services.AddMassTransit(config =>
            {
                // Enregistrer tous les Consumers automatiquement
                // MassTransit va scanner l'assembly et trouver :
                // - ProductPriceChangedConsumer
                // - ProductStockChangedConsumer
                // - ProductDeletedConsumer
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
                    // MassTransit créera automatiquement les queues :
                    // - basket-service-product-price-changed
                    // - basket-service-product-stock-changed
                    // - basket-service-product-deleted
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}