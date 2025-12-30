using System.Reflection;
using Identity.Application.Common.Interfaces;
using Identity.Infrastructure.Data;
using Identity.Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure
{
    /// <summary>
    /// Dependency Injection configuration for the Infrastructure layer
    /// Registers all services: DbContext, Implementations, MassTransit
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

            services.AddDbContext<IdentityDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("IdentityDb");

                options.UseSqlServer(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });

                // Enable detailed logging in development
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // Register IIdentityDbContext interface
            services.AddScoped<IIdentityDbContext>(provider =>
                provider.GetRequiredService<IdentityDbContext>());

            // ====================================
            // 2. CONFIGURATION SETTINGS
            // ====================================

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // ====================================
            // 3. APPLICATION SERVICES (Implementations)
            // ====================================

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEmailService, EmailService>();

            // Register HttpContextAccessor for CurrentUserService
            services.AddHttpContextAccessor();

            // ====================================
            // 4. MASSTRANSIT + RABBITMQ (Messaging)
            // ====================================

            services.AddMassTransit(config =>
            {
                // Register all domain event handlers automatically
                // MassTransit will scan the assembly and find all handlers
                config.AddConsumers(Assembly.GetExecutingAssembly());

                // Configure RabbitMQ
                config.UsingRabbitMq((context, cfg) =>
                {
                    // Get RabbitMQ settings from appsettings.json
                    var rabbitMqHost = configuration["RabbitMQSettings:Host"] ?? "localhost";
                    var rabbitMqUsername = configuration["RabbitMQSettings:Username"] ?? "guest";
                    var rabbitMqPassword = configuration["RabbitMQSettings:Password"] ?? "guest";

                    cfg.Host(rabbitMqHost, h =>
                    {
                        h.Username(rabbitMqUsername);
                        h.Password(rabbitMqPassword);
                    });

                    // Configure endpoints (queues)
                    // MassTransit will automatically create queues for the identity service
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
