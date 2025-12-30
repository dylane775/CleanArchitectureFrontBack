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

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        /// <returns>HTML page with confirmation result</returns>
        [HttpGet("confirm-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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

                // Return success HTML page
                var successHtml = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>Email Confirmé - eShop</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }
        .container {
            background: white;
            padding: 40px;
            border-radius: 10px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            text-align: center;
            max-width: 500px;
        }
        .success-icon {
            width: 80px;
            height: 80px;
            background: #4CAF50;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 20px;
            animation: scaleIn 0.5s ease-out;
        }
        .success-icon::after {
            content: '✓';
            color: white;
            font-size: 50px;
            font-weight: bold;
        }
        h1 {
            color: #333;
            margin-bottom: 10px;
        }
        p {
            color: #666;
            line-height: 1.6;
            margin-bottom: 30px;
        }
        .button {
            display: inline-block;
            padding: 12px 30px;
            background: #4CAF50;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            transition: background 0.3s;
        }
        .button:hover {
            background: #45a049;
        }
        @keyframes scaleIn {
            from { transform: scale(0); }
            to { transform: scale(1); }
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='success-icon'></div>
        <h1>Email Confirmé avec Succès! 🎉</h1>
        <p>Votre adresse email a été vérifiée. Vous pouvez maintenant vous connecter à votre compte eShop et profiter de tous nos services.</p>
        <a href='http://localhost:5245/swagger' class='button'>Aller à l'API</a>
    </div>
</body>
</html>";

                return Content(successHtml, "text/html");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Email confirmation failed for user {UserId}", userId);

                // Return error HTML page
                var errorHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>Erreur de Confirmation - eShop</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }}
        .container {{
            background: white;
            padding: 40px;
            border-radius: 10px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            text-align: center;
            max-width: 500px;
        }}
        .error-icon {{
            width: 80px;
            height: 80px;
            background: #f44336;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 20px;
        }}
        .error-icon::after {{
            content: '✕';
            color: white;
            font-size: 50px;
            font-weight: bold;
        }}
        h1 {{
            color: #333;
            margin-bottom: 10px;
        }}
        p {{
            color: #666;
            line-height: 1.6;
            margin-bottom: 30px;
        }}
        .error-message {{
            background: #ffebee;
            border-left: 4px solid #f44336;
            padding: 15px;
            margin: 20px 0;
            text-align: left;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='error-icon'></div>
        <h1>Erreur de Confirmation</h1>
        <div class='error-message'>
            <strong>Erreur:</strong> {ex.Message}
        </div>
        <p>Si le problème persiste, veuillez contacter le support ou demander un nouveau lien de confirmation.</p>
    </div>
</body>
</html>";

                return Content(errorHtml, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during email confirmation");

                var errorHtml = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>Erreur - eShop</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }
        .container {
            background: white;
            padding: 40px;
            border-radius: 10px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            text-align: center;
            max-width: 500px;
        }
        .error-icon {
            width: 80px;
            height: 80px;
            background: #f44336;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 20px;
        }
        .error-icon::after {
            content: '✕';
            color: white;
            font-size: 50px;
            font-weight: bold;
        }
        h1 {
            color: #333;
            margin-bottom: 10px;
        }
        p {
            color: #666;
            line-height: 1.6;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='error-icon'></div>
        <h1>Une Erreur est Survenue</h1>
        <p>Nous sommes désolés, une erreur inattendue s'est produite lors de la confirmation de votre email. Veuillez réessayer plus tard ou contacter le support.</p>
    </div>
</body>
</html>";

                return Content(errorHtml, "text/html");
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
