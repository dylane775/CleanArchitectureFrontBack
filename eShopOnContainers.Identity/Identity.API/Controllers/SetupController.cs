using Identity.Application.Common.Interfaces;
using Identity.Domain.Entities;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Controllers
{
    /// <summary>
    /// Controller for initial system setup
    /// This endpoint is only available when no admin user exists
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SetupController : ControllerBase
    {
        private readonly IdentityDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<SetupController> _logger;

        public SetupController(
            IdentityDbContext context,
            IPasswordHasher passwordHasher,
            ILogger<SetupController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates the first admin user
        /// This endpoint only works if NO admin user exists in the system
        /// </summary>
        /// <param name="dto">Admin user credentials</param>
        /// <returns>Success message</returns>
        [HttpPost("create-first-admin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateFirstAdmin([FromBody] CreateFirstAdminDto dto)
        {
            try
            {
                // Check if Admin role exists
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole == null)
                {
                    return BadRequest(new { Message = "Admin role does not exist. Please seed the database first." });
                }

                // Check if any admin user already exists
                var existingAdmins = await _context.Users
                    .Include(u => u.Roles)
                    .Where(u => u.Roles.Any(r => r.Name == "Admin"))
                    .AnyAsync();

                if (existingAdmins)
                {
                    _logger.LogWarning("Attempt to create first admin when admin already exists");
                    return StatusCode(403, new { Message = "Setup is disabled. An admin user already exists in the system." });
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
                {
                    return BadRequest(new { Message = "Valid email is required" });
                }

                if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
                {
                    return BadRequest(new { Message = "Password must be at least 8 characters long" });
                }

                // Hash password
                var passwordHash = _passwordHasher.HashPassword(dto.Password);

                // Create admin user
                var adminUser = new User(
                    email: dto.Email.ToLowerInvariant(),
                    passwordHash: passwordHash,
                    firstName: dto.FirstName ?? "System",
                    lastName: dto.LastName ?? "Administrator",
                    phoneNumber: dto.PhoneNumber
                );

                // Confirm email automatically for first admin
                adminUser.ConfirmEmail(adminUser.EmailConfirmationToken);

                // Assign Admin role
                adminUser.AddRole(adminRole);

                // Save to database
                await _context.Users.AddAsync(adminUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("First admin user created successfully: {Email}", dto.Email);

                return Ok(new
                {
                    Message = "First admin user created successfully",
                    Email = dto.Email,
                    UserId = adminUser.Id,
                    Warning = "This endpoint is now disabled. No more admin users can be created via this endpoint."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating first admin");
                return StatusCode(500, new { Message = "An error occurred while creating the admin user" });
            }
        }

        /// <summary>
        /// Checks if the system needs initial setup
        /// </summary>
        /// <returns>Setup status</returns>
        [HttpGet("status")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSetupStatus()
        {
            try
            {
                // Check if Admin role exists
                var adminRoleExists = await _context.Roles.AnyAsync(r => r.Name == "Admin");

                // Check if any admin user exists
                var adminUserExists = await _context.Users
                    .Include(u => u.Roles)
                    .Where(u => u.Roles.Any(r => r.Name == "Admin"))
                    .AnyAsync();

                var needsSetup = !adminRoleExists || !adminUserExists;

                return Ok(new
                {
                    NeedsSetup = needsSetup,
                    AdminRoleExists = adminRoleExists,
                    AdminUserExists = adminUserExists,
                    Message = needsSetup
                        ? "System needs initial setup. Use POST /api/Setup/create-first-admin to create the first admin user."
                        : "System is already set up. Setup endpoint is disabled."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking setup status");
                return StatusCode(500, new { Message = "An error occurred while checking setup status" });
            }
        }
    }

    /// <summary>
    /// DTO for creating the first admin user
    /// </summary>
    public class CreateFirstAdminDto
    {
        /// <summary>
        /// Admin email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Admin password (min 8 characters)
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Admin first name
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Admin last name
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Admin phone number (optional)
        /// </summary>
        public string? PhoneNumber { get; set; }
    }
}
