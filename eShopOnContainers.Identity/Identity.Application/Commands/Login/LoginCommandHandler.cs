using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.Common.Interfaces;
using Identity.Application.DTOs.Output;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Commands.Login
{
    /// <summary>
    /// Handler for LoginCommand
    /// </summary>
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        private readonly IIdentityDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public LoginCommandHandler(
            IIdentityDbContext context,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Find user by email
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Verify password
            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("User account is inactive");
            }

            // Check if email is confirmed
            if (!user.IsEmailConfirmed)
            {
                throw new UnauthorizedAccessException("Email is not confirmed. Please confirm your email before logging in.");
            }

            // Record login
            user.RecordLogin(request.IpAddress);

            // Generate access token
            var accessToken = _tokenService.GenerateAccessToken(user);

            // Generate refresh token
            var refreshTokenString = _tokenService.GenerateRefreshToken();

            // Calculate expiration date for refresh token
            var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_tokenService.GetRefreshTokenExpirationDays());

            // Add refresh token to user
            user.AddRefreshToken(refreshTokenString, refreshTokenExpiresAt, request.IpAddress);

            // Save changes using UnitOfWork (publishes domain events)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Calculate access token expiration
            var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetAccessTokenExpirationMinutes());

            // Return authentication response
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
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
