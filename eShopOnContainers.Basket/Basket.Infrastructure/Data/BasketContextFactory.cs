using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Basket.Infrastructure.Data
{
    /// <summary>
    /// Factory pour créer le DbContext au moment du design (migrations)
    /// EF Core utilise cette classe pour générer les migrations
    /// </summary>
    public class BasketContextFactory : IDesignTimeDbContextFactory<BasketContext>
    {
        public BasketContext CreateDbContext(string[] args)
        {
            // Charger la configuration depuis appsettings.json
            // IMPORTANT : cherche dans le répertoire courant
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Récupérer la connection string
            var connectionString = configuration.GetConnectionString("BasketDb");

            // Créer les options du DbContext
            var optionsBuilder = new DbContextOptionsBuilder<BasketContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Retourner le DbContext
            return new BasketContext(optionsBuilder.Options);
        }
    }
}