using System;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Commands.ConfirmEmail
{
    /// <summary>
    /// Handler for ConfirmEmailCommand
    /// </summary>
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, bool>
    {
        private readonly IIdentityDbContext _context;
        private readonly IEmailService _emailService;

        public ConfirmEmailCommandHandler(
            IIdentityDbContext context,
            IEmailService emailService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            // Find user by ID
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{request.UserId}' not found");
            }

            // Confirm email with token validation
            user.ConfirmEmail(request.ConfirmationToken);

            // Save changes
            await _context.SaveChangesAsync(cancellationToken);

            // Send welcome email
            try
            {
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);
            }
            catch (Exception ex)
            {
                // Log but don't fail confirmation if welcome email fails
                Console.WriteLine($"Failed to send welcome email: {ex.Message}");
            }

            // EmailConfirmedDomainEvent is raised automatically in the ConfirmEmail method
            return true;
        }
    }
}
