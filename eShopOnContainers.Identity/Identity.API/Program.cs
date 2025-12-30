using Identity.Application;
using Identity.Infrastructure;
using Identity.Infrastructure.Data;
using Identity.API.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ====================================
// 1. CONFIGURATION DES SERVICES
// ====================================

// Controllers (API REST)
builder.Services.AddControllers();

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Identity API",
        Version = "v1",
        Description = "Identity and Authentication Service for eShopOnContainers"
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// APPLICATION LAYER (MediatR, Validators, AutoMapper, Behaviors)
// Pass Infrastructure assembly to scan for Domain Event Handlers
builder.Services.AddApplication(typeof(Identity.Infrastructure.DependencyInjection).Assembly);

// INFRASTRUCTURE LAYER (DbContext, Repositories, UnitOfWork, MassTransit)
builder.Services.AddInfrastructure(builder.Configuration);

// JWT AUTHENTICATION
var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// CORS - Configure pour accepter le frontend Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev server
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Important pour les cookies/credentials
    });
});

// ====================================
// 2. CONSTRUCTION DE L'APPLICATION
// ====================================

var app = builder.Build();

// ====================================
// 3. MIGRATION AUTOMATIQUE
// ====================================

// Apply migrations and seed data automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<IdentityDbContext>();
    var passwordHasher = services.GetRequiredService<Identity.Application.Common.Interfaces.IPasswordHasher>();
    var logger = services.GetRequiredService<ILogger<IdentityDbContextSeed>>();

    try
    {
        // Apply migrations
        dbContext.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully");

        // Seed initial data (roles and admin user)
        var seeder = new IdentityDbContextSeed(dbContext, passwordHasher, logger);
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during database initialization: {ex.Message}");
        logger.LogError(ex, "An error occurred during database initialization");
    }
}

// ====================================
// 4. CONFIGURATION DU PIPELINE HTTP
// ====================================

// Global Exception Handling Middleware
app.UseExceptionHandlingMiddleware();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Mapping des Controllers
app.MapControllers();

// ====================================
// 5. HEALTH CHECK ENDPOINT
// ====================================

app.MapGet("/health", async (IdentityDbContext context) =>
{
    try
    {
        // Check database connection
        await context.Database.CanConnectAsync();

        return Results.Ok(new
        {
            Status = "Healthy",
            Service = "Identity API",
            Timestamp = DateTime.UtcNow,
            Database = "Connected"
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            Status = "Unhealthy",
            Service = "Identity API",
            Timestamp = DateTime.UtcNow,
            Database = "Disconnected",
            Error = ex.Message
        });
    }
})
.WithName("HealthCheck")
.WithTags("Health");

// ====================================
// 6. INFO ENDPOINT
// ====================================

app.MapGet("/info", () =>
{
    return Results.Ok(new
    {
        Service = "Identity API",
        Version = "1.0.0",
        Environment = builder.Environment.EnvironmentName,
        Architecture = "Clean Architecture",
        Patterns = new[] { "CQRS", "Repository", "Unit of Work", "Domain Events", "Event-Driven" },
        Layers = new[] { "Domain", "Application", "Infrastructure", "API" },
        Features = new[] { "JWT Authentication", "User Management", "Role Management", "Token Refresh" }
    });
})
.WithName("GetInfo")
.WithTags("Info");

// ====================================
// 7. DEMARRAGE DE L'APPLICATION
// ====================================

Console.WriteLine("Identity API started successfully!");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Swagger UI: http://localhost:5245/swagger");

app.Run();
