using System;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Commands.RemoveRole
{
    /// <summary>
    /// Handler for RemoveRoleCommand
    /// </summary>
    public class RemoveRoleCommandHandler : IRequestHandler<RemoveRoleCommand, bool>
    {
        private readonly IIdentityDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveRoleCommandHandler(
            IIdentityDbContext context,
            IUnitOfWork unitOfWork)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<bool> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
        {
            // Find user by ID
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{request.UserId}' not found");
            }

            // Find role by name
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == request.RoleName, cancellationToken);

            if (role == null)
            {
                throw new InvalidOperationException($"Role '{request.RoleName}' not found");
            }

            // Remove role from user
            user.RemoveRole(role);

            // Save changes using UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
