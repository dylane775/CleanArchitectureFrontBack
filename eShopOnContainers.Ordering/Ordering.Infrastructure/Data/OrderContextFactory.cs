using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Ordering.Infrastructure.Data
{
    public class OrderContextFactory : IDesignTimeDbContextFactory<OrderContext>
    {
        public OrderContext CreateDbContext(string[] args)
        {
            // Pointer vers le répertoire de l'API où se trouve appsettings.json
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../Ordering.API");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("OrderDb");

            Console.WriteLine($"Connection String: {connectionString}"); // Debug

            var optionsBuilder = new DbContextOptionsBuilder<OrderContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new OrderContext(optionsBuilder.Options);
        }
    }
}