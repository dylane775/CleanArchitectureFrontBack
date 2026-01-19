using Payment.Application;
using Payment.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Data;
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
        Title = "Payment API",
        Version = "v1",
        Description = "Payment Service for eShopOnContainers - Integrates Monetbil payment gateway"
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

// ✅ APPLICATION LAYER (MediatR, Validators, AutoMapper, Behaviors)
// Passer l'assembly Infrastructure pour scanner les Domain Event Handlers
builder.Services.AddApplication(typeof(Payment.Infrastructure.DependencyInjection).Assembly);

// ✅ INFRASTRUCTURE LAYER (DbContext, Repositories, UnitOfWork, PaymentGateway, MassTransit)
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
            ClockSkew = TimeSpan.Zero
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

// Applique automatiquement les migrations au démarrage
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentContext>();
    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("✅ Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error applying migrations: {ex.Message}");
    }
}

// ====================================
// 4. CONFIGURATION DU PIPELINE HTTP
// ====================================

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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

app.MapGet("/health", async (PaymentContext context) =>
{
    try
    {
        // Vérifier la connexion à la base de données
        await context.Database.CanConnectAsync();

        return Results.Ok(new
        {
            Status = "Healthy",
            Service = "Payment API",
            Timestamp = DateTime.UtcNow,
            Database = "Connected"
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            Status = "Unhealthy",
            Service = "Payment API",
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
        Service = "Payment API",
        Version = "1.0.0",
        Environment = builder.Environment.EnvironmentName,
        Architecture = "Clean Architecture",
        Patterns = new[] { "CQRS", "Repository", "Unit of Work", "Domain Events", "Event-Driven" },
        Layers = new[] { "Domain", "Application", "Infrastructure", "API" },
        PaymentProvider = "Monetbil"
    });
})
.WithName("GetInfo")
.WithTags("Info");

// ====================================
// 7. DÉMARRAGE DE L'APPLICATION
// ====================================

Console.WriteLine("🚀 Payment API démarrée avec succès !");
Console.WriteLine($"📍 Environnement : {builder.Environment.EnvironmentName}");
Console.WriteLine($"🔗 Swagger UI : http://localhost:5246/swagger");
Console.WriteLine($"💳 Payment Provider : Monetbil");

app.Run();
