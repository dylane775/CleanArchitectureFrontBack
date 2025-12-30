using System.Reflection;
using Identity.Application.Common.Interfaces;
using Identity.Domain.Common;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Data
{
    /// <summary>
    /// Database context for the Identity bounded context
    /// Implements IIdentityDbContext interface from Application layer
    /// </summary>
    public class IdentityDbContext : DbContext, IIdentityDbContext
    {
        /// <summary>
        /// Users table
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// Roles table
        /// </summary>
        public DbSet<Role> Roles { get; set; } = null!;

        /// <summary>
        /// Refresh tokens table
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        /// <summary>
        /// Constructor for dependency injection
        /// </summary>
        /// <param name="options">DbContext options</param>
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Protected constructor for EF Core tools
        /// </summary>
        protected IdentityDbContext()
        {
        }

        /// <summary>
        /// Configures the database schema and entity relationships
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignore domain events - they are not entities to persist
            modelBuilder.Ignore<BaseDomainEvent>();

            // Apply all entity configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Saves all pending changes to the database
        /// Handles domain events before saving
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of affected rows</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Fix entity states: If an entity is Modified but has never been saved (ModifiedAt is null),
            // it's actually a new entity that should be Added
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Modified && e.Entity.ModifiedAt == null);

            foreach (var entry in entries)
            {
                // This is a new entity that was incorrectly tracked as Modified
                entry.State = EntityState.Added;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
