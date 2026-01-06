using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Identity.Application.Commands.Register;
using Identity.Application.Commands.Login;
using Identity.Application.Commands.RefreshToken;
using Identity.Application.Commands.ConfirmEmail;
using Identity.Application.DTOs.Input;
using Identity.Application.DTOs.Output;
using Identity.Application.Common.Interfaces;
using Identity.Application.Queries.GetUserById;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Controllers
{
    /// <summary>
    /// Handles authentication operations including registration, login, token refresh, and email confirmation
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;
        private readonly ITokenService _tokenService;
        private readonly IIdentityDbContext _context;

        public AuthController(
            IMediator mediator,
            ILogger<AuthController> logger,
            ITokenService tokenService,
            IIdentityDbContext context)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ====================================
        // AUTHENTICATION ENDPOINTS
        // ====================================

        /// <summary>
        /// Registers a new user account
        /// </summary>
        /// <param name="dto">User registration information</param>
        /// <returns>The created user's ID</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> RegisterUser([FromBody] RegisterDto dto)
        {
            _logger.LogInformation("Registering new user with email {Email}", dto.Email);

            try
            {
                var command = new RegisterCommand
                {
                    Email = dto.Email,
                    Password = dto.Password,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.PhoneNumber
                };

                var userId = await _mediator.Send(command);

                _logger.LogInformation("User {UserId} registered successfully with email {Email}", userId, dto.Email);

                return CreatedAtRoute("GetUserById", new { id = userId }, userId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registration failed for email {Email}", dto.Email);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration");
                return BadRequest(new { Message = "An error occurred during registration" });
            }
        }

        /// <summary>
        /// Authenticates a user and returns access and refresh tokens
        /// </summary>
        /// <param name="dto">Login credentials</param>
        /// <returns>Authentication response with tokens</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            _logger.LogInformation("Login attempt for email {Email}", dto.Email);

            try
            {
                // Extract IP address from HTTP context
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                var command = new LoginCommand
                {
                    Email = dto.Email,
                    Password = dto.Password,
                    IpAddress = ipAddress
                };

                var result = await _mediator.Send(command);

                _logger.LogInformation("User {Email} logged in successfully", dto.Email);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Login failed for email {Email}", dto.Email);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Login failed for email {Email}", dto.Email);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login");
                return BadRequest(new { Message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token
        /// </summary>
        /// <param name="dto">Refresh token information</param>
        /// <returns>New access and refresh tokens</returns>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            _logger.LogInformation("Refresh token request received");

            try
            {
                // Extract IP address from HTTP context
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                var command = new RefreshTokenCommand
                {
                    RefreshToken = dto.RefreshToken,
                    IpAddress = ipAddress
                };

                var result = await _mediator.Send(command);

                _logger.LogInformation("Token refreshed successfully");

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Token refresh failed");
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                return BadRequest(new { Message = "An error occurred during token refresh" });
            }
        }

        /// <summary>
        /// Confirms a user's email address using the confirmation token (via clickable link in email)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="token">Confirmation token</param>
        /// <returns>Redirect to frontend with authentication tokens</returns>
        [HttpGet("confirm-email")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmailViaLink([FromQuery] Guid userId, [FromQuery] string token)
        {
            _logger.LogInformation("Email confirmation request via link for user {UserId}", userId);

            try
            {
                var command = new ConfirmEmailCommand
                {
                    UserId = userId,
                    ConfirmationToken = token
                };

                await _mediator.Send(command);

                _logger.LogInformation("Email confirmed successfully for user {UserId}", userId);

                // Get user and generate authentication tokens
                var user = await _context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    throw new InvalidOperationException("User not found after confirmation");
                }

                // Generate tokens
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshTokenString = _tokenService.GenerateRefreshToken();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                // Create refresh token entity using the constructor
                var refreshTokenEntity = new Identity.Domain.Entities.RefreshToken(
                    refreshTokenString,
                    userId,
                    DateTime.UtcNow.AddDays(7),
                    ipAddress
                );

                // Save refresh token
                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync(default);

                // Redirect to frontend with tokens
                var frontendUrl = "http://localhost:4200/auth/confirm-success";
                var redirectUrl = $"{frontendUrl}?accessToken={Uri.EscapeDataString(accessToken)}&refreshToken={Uri.EscapeDataString(refreshTokenString)}";

                return Redirect(redirectUrl);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Email confirmation failed for user {UserId}", userId);

                // Redirect to frontend error page with error message
                var frontendUrl = "http://localhost:4200/auth/confirm-error";
                var redirectUrl = $"{frontendUrl}?error={Uri.EscapeDataString(ex.Message)}";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during email confirmation");

                // Redirect to frontend error page
                var frontendUrl = "http://localhost:4200/auth/confirm-error";
                var redirectUrl = $"{frontendUrl}?error=An unexpected error occurred";
                return Redirect(redirectUrl);
            }
        }


        /// <summary>
        /// Confirms a user's email address using the confirmation token (POST endpoint for API calls)
        /// </summary>
        /// <param name="dto">Email confirmation information</param>
        /// <returns>No content on success</returns>
        [HttpPost("confirm-email")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
        {
            _logger.LogInformation("Email confirmation request for user {UserId}", dto.UserId);

            try
            {
                var command = new ConfirmEmailCommand
                {
                    UserId = dto.UserId,
                    ConfirmationToken = dto.ConfirmationToken
                };

                await _mediator.Send(command);

                _logger.LogInformation("Email confirmed successfully for user {UserId}", dto.UserId);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Email confirmation failed for user {UserId}", dto.UserId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during email confirmation");
                return BadRequest(new { Message = "An error occurred during email confirmation" });
            }
        }

        /// <summary>
        /// Logs out the current user (placeholder for token invalidation)
        /// </summary>
        /// <returns>No content on success</returns>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout()
        {
            // Get user ID from claims if authenticated
            var userIdClaim = User?.FindFirst("sub")?.Value ?? User?.FindFirst("userId")?.Value;

            if (!string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogInformation("User {UserId} logged out", userIdClaim);
            }
            else
            {
                _logger.LogInformation("Logout request received (user not authenticated)");
            }

            // In a production environment, you would:
            // 1. Invalidate the refresh token in the database
            // 2. Add the access token to a blacklist (if using token blacklisting)
            // 3. Clear any server-side session data

            await Task.CompletedTask; // Placeholder for async operations

            return NoContent();
        }
    }
}
