using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Catalog.Infrastructure.Data.migrations
{
    /// <summary>
    /// Factory pour créer le DbContext au moment du design (migrations)
    /// EF Core utilise cette classe pour générer les migrations
    /// </summary>
    public class CatalogContextFactory : IDesignTimeDbContextFactory<CatalogContext>
    {
        public CatalogContext CreateDbContext(string[] args)
        {
            // Charger la configuration depuis appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Récupérer la connection string
            var connectionString = configuration.GetConnectionString("CatalogDb");

            // Créer les options du DbContext
            var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Retourner le DbContext
            return new CatalogContext(optionsBuilder.Options);
        }
    }
}