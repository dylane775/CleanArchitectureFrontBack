using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.Common.Interfaces;
using Identity.Application.DTOs.Output;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Commands.RefreshToken
{
    /// <summary>
    /// Handler for RefreshTokenCommand
    /// </summary>
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
    {
        private readonly IIdentityDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenCommandHandler(
            IIdentityDbContext context,
            ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Find the refresh token
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u!.Roles)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

            if (refreshToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            // Check if token is still active
            if (!refreshToken.IsActive)
            {
                throw new UnauthorizedAccessException("Refresh token is no longer active");
            }

            var user = refreshToken.User;

            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("User account is inactive");
            }

            // Generate new access token
            var newAccessToken = _tokenService.GenerateAccessToken(user);

            // Generate new refresh token
            var newRefreshTokenString = _tokenService.GenerateRefreshToken();

            // Calculate expiration date for new refresh token
            var newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_tokenService.GetRefreshTokenExpirationDays());

            // Revoke the old refresh token and replace it with the new one
            user.RevokeRefreshToken(request.RefreshToken, request.IpAddress, newRefreshTokenString);

            // Add new refresh token to user
            user.AddRefreshToken(newRefreshTokenString, newRefreshTokenExpiresAt, request.IpAddress);

            // Save changes using UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Calculate access token expiration
            var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetAccessTokenExpirationMinutes());

            // Return authentication response
            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenString,
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = user.Roles.Select(r => r.Name).ToList(),
                ExpiresAt = accessTokenExpiresAt
            };
        }
    }
}
