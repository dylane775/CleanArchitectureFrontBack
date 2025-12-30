using System;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Commands.AssignRole
{
    /// <summary>
    /// Handler for AssignRoleCommand
    /// </summary>
    public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, bool>
    {
        private readonly IIdentityDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public AssignRoleCommandHandler(
            IIdentityDbContext context,
            IUnitOfWork unitOfWork)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<bool> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
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

            // Assign role to user
            user.AddRole(role);

            // Save changes using UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
