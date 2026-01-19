using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Catalog.Application.common.Interfaces;
using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Repositories;
using Catalog.Infrastructure.Data;
using Catalog.Infrastructure.Data.Repositories;
using Catalog.Infrastructure.Services;
using MassTransit;
using System.Reflection;

namespace Catalog.Infrastructure
{
    /// <summary>
    /// Configuration de l'injection de dépendances pour la couche Infrastructure
    /// Enregistre tous les services : DbContext, Repositories, UnitOfWork, MediatR Handlers
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
            
            services.AddDbContext<CatalogContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("CatalogDb");
                
                options.UseSqlServer(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(CatalogContext).Assembly.FullName);
                    });

                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // ====================================
            // 2. REPOSITORIES (Implémentations)
            // ====================================

            services.AddScoped<ICatalogRepository, CatalogRepository>();
            services.AddScoped<ICatalogTypeRepository, CatalogTypeRepository>();
            services.AddScoped<ICatalogBrandRepository, CatalogBrandRepository>();
            services.AddScoped<IProductReviewRepository, ProductReviewRepository>();

            // ====================================
            // 3. UNIT OF WORK (Transactions + Events)
            // ====================================

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ====================================
            // 3.5 FILE STORAGE SERVICE
            // ====================================

            services.AddScoped<IFileStorageService, LocalFileStorageService>();

            // ====================================
            // 4. MEDIATR DOMAIN EVENT HANDLERS ✅ AJOUTÉ
            // ====================================
            
            // Enregistrer les Domain Event Handlers de l'Infrastructure
            // (ProductPriceChangedDomainEventHandler, ProductStockUpdatedDomainEventHandler)
            services.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // ====================================
            // 5. MASSTRANSIT + RABBITMQ (Messaging)
            // ====================================
            
            services.AddMassTransit(config =>
            {
                // Enregistrer tous les Domain Event Handlers (Consumers)
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

                    // Configuration automatique des endpoints
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}