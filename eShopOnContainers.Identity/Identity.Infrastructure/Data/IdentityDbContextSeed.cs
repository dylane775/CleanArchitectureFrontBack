using Identity.Application.Common.Interfaces;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Data
{
    /// <summary>
    /// Seeds initial data for the Identity database
    /// </summary>
    public class IdentityDbContextSeed
    {
        private readonly IdentityDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<IdentityDbContextSeed> _logger;

        public IdentityDbContextSeed(
            IdentityDbContext context,
            IPasswordHasher passwordHasher,
            ILogger<IdentityDbContextSeed> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Seeds initial data including roles and admin user
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                // Seed Roles first
                await SeedRolesAsync();
                await _context.SaveChangesAsync(); // Save roles first

                // Then seed Admin user
                await SeedAdminUserAsync();
                await _context.SaveChangesAsync(); // Save admin user separately

                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        /// <summary>
        /// Seeds default roles
        /// </summary>
        private async Task SeedRolesAsync()
        {
            // Check if roles already exist
            if (await _context.Roles.AnyAsync())
            {
                _logger.LogInformation("Roles already exist, skipping role seeding");
                return;
            }

            _logger.LogInformation("Seeding roles...");

            var roles = new[]
            {
                new Role("Admin", "Administrator with full system access", "users.manage,roles.manage,orders.manage,products.manage"),
                new Role("Customer", "Regular customer with basic access", "orders.view,orders.create,profile.manage"),
                new Role("Manager", "Manager with elevated permissions", "orders.manage,products.manage,reports.view")
            };

            await _context.Roles.AddRangeAsync(roles);

            _logger.LogInformation("Seeded {Count} roles", roles.Length);
        }

        /// <summary>
        /// Seeds default admin user
        /// </summary>
        private async Task SeedAdminUserAsync()
        {
            const string adminEmail = "admin@eshop.com";

            // Check if admin user already exists
            if (await _context.Users.AnyAsync(u => u.Email == adminEmail))
            {
                _logger.LogInformation("Admin user already exists, skipping admin user seeding");
                return;
            }

            _logger.LogInformation("Seeding admin user...");

            // Hash the default admin password
            var passwordHash = _passwordHasher.HashPassword("Admin@123");

            // Create admin user
            var adminUser = new User(
                email: adminEmail,
                passwordHash: passwordHash,
                firstName: "System",
                lastName: "Administrator",
                phoneNumber: null
            );

            // Mark email as confirmed (skip email confirmation for seeded admin)
            adminUser.ConfirmEmail(adminUser.EmailConfirmationToken);

            // Save user first
            await _context.Users.AddAsync(adminUser);
            await _context.SaveChangesAsync();

            // Find Admin role
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                // Add role to user
                adminUser.AddRole(adminRole);

                // Mark user as modified to ensure EF tracks the change
                _context.Users.Update(adminUser);
            }

            _logger.LogInformation("Admin user created: {Email} / Password: Admin@123", adminEmail);
            _logger.LogWarning("SECURITY WARNING: Change the default admin password immediately in production!");
        }
    }
}
