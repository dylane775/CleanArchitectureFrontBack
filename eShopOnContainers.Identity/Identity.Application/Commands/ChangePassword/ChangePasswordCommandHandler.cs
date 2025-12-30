using System;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Commands.ChangePassword
{
    /// <summary>
    /// Handler for ChangePasswordCommand
    /// </summary>
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        private readonly IIdentityDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public ChangePasswordCommandHandler(
            IIdentityDbContext context,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            // Find user by ID
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{request.UserId}' not found");
            }

            // Verify current password
            if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Current password is incorrect");
            }

            // Hash new password
            var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);

            // Change password
            user.ChangePassword(newPasswordHash);

            // Save changes using UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // All refresh tokens are revoked automatically in the ChangePassword method
            return true;
        }
    }
}
