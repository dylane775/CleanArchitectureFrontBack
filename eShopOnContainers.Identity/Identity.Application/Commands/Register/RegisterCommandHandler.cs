using System;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.Common.Interfaces;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Commands.Register
{
    /// <summary>
    /// Handler for RegisterCommand
    /// </summary>
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Guid>
    {
        private readonly IIdentityDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;

        public RegisterCommandHandler(
            IIdentityDbContext context,
            IPasswordHasher passwordHasher,
            IEmailService emailService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Check if user with this email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email '{request.Email}' already exists");
            }

            // Hash the password
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            // Create new user
            var user = new User(
                request.Email,
                passwordHash,
                request.FirstName,
                request.LastName,
                request.PhoneNumber
            );

            // Add default Customer role
            var customerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == Role.Customer, cancellationToken);

            if (customerRole != null)
            {
                user.AddRole(customerRole);
            }

            // Add user to context
            _context.Users.Add(user);

            // Save changes
            await _context.SaveChangesAsync(cancellationToken);

            // Send email confirmation
            try
            {
                await _emailService.SendEmailConfirmationAsync(
                    user.Email,
                    user.FirstName,
                    user.EmailConfirmationToken,
                    user.Id.ToString());
            }
            catch (Exception ex)
            {
                // Log but don't fail registration if email fails
                // The user can request a new confirmation email later
                Console.WriteLine($"Failed to send confirmation email: {ex.Message}");
            }

            // UserRegisteredDomainEvent is raised automatically in the User constructor
            return user.Id;
        }
    }
}
