using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Identity.Infrastructure.Data
{
    /// <summary>
    /// Factory for creating the DbContext at design time (migrations)
    /// EF Core uses this class to generate migrations
    /// </summary>
    public class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
    {
        public IdentityDbContext CreateDbContext(string[] args)
        {
            // Load configuration from appsettings.json in the API project
            var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Identity.API");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            // Get the connection string
            var connectionString = configuration.GetConnectionString("IdentityDb");

            // Create DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Return the DbContext
            return new IdentityDbContext(optionsBuilder.Options);
        }
    }
}
