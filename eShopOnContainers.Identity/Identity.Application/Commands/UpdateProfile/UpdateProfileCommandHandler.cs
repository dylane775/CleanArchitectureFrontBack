using System;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Commands.UpdateProfile
{
    /// <summary>
    /// Handler for UpdateProfileCommand
    /// </summary>
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, bool>
    {
        private readonly IIdentityDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProfileCommandHandler(
            IIdentityDbContext context,
            IUnitOfWork unitOfWork)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            // Find user by ID
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{request.UserId}' not found");
            }

            // Update profile
            user.UpdateProfile(
                request.FirstName,
                request.LastName,
                request.PhoneNumber
            );

            // Save changes using UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
