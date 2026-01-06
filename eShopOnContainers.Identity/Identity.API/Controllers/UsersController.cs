using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Identity.Application.Commands.UpdateProfile;
using Identity.Application.Commands.ChangePassword;
using Identity.Application.Queries.GetUserById;
using Identity.Application.Queries.GetAllUsers;
using Identity.Application.Queries.GetUserRoles;
using Identity.Application.Queries.GetUserByEmail;
using Identity.Application.DTOs.Input;
using Identity.Application.DTOs.Output;
using Identity.Application.Common.Models;

namespace Identity.API.Controllers
{
    /// <summary>
    /// Handles user management operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IMediator mediator, ILogger<UsersController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ====================================
        // QUERIES (Read Operations)
        // ====================================

        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        /// <param name="id">The user's ID</param>
        /// <returns>User information</returns>
        [HttpGet("{id:guid}", Name = "GetUserById")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            _logger.LogInformation("Getting user {UserId}", id);

            try
            {
                var query = new GetUserByIdQuery { UserId = id };
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not found", id);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user {UserId}", id);
                return BadRequest(new { Message = "An error occurred while retrieving user" });
            }
        }

        /// <summary>
        /// Gets the current authenticated user's information
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            // Extract user ID from JWT claims
            var userIdClaim = User?.FindFirst("sub")?.Value ?? User?.FindFirst("userId")?.Value;
            var emailClaim = User?.FindFirst("email")?.Value ?? User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) && string.IsNullOrEmpty(emailClaim))
            {
                _logger.LogWarning("No user ID or email found in claims");
                return Unauthorized(new { Message = "User not authenticated" });
            }

            _logger.LogInformation("Getting current user information");

            try
            {
                UserDto result;

                // Try to get by user ID first
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    var query = new GetUserByIdQuery { UserId = userId };
                    result = await _mediator.Send(query);
                }
                // Fallback to email
                else if (!string.IsNullOrEmpty(emailClaim))
                {
                    var query = new GetUserByEmailQuery { Email = emailClaim };
                    result = await _mediator.Send(query);
                }
                else
                {
                    return Unauthorized(new { Message = "Invalid authentication claims" });
                }

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Current user not found");
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting current user");
                return BadRequest(new { Message = "An error occurred while retrieving user information" });
            }
        }

        /// <summary>
        /// Gets all users with optional pagination and filtering
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <param name="isActive">Filter by active status (optional)</param>
        /// <returns>Paginated list of users</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedItems<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PaginatedItems<UserDto>>> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null)
        {
            _logger.LogInformation("Getting all users (page: {Page}, pageSize: {PageSize}, isActive: {IsActive})",
                page, pageSize, isActive);

            try
            {
                var query = new GetAllUsersQuery
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    IsActive = isActive
                };

                var result = await _mediator.Send(query);

                _logger.LogInformation("Retrieved {Count} of {Total} users", result.Data.Count(), result.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting users");
                return BadRequest(new { Message = "An error occurred while retrieving users" });
            }
        }

        /// <summary>
        /// Gets the roles assigned to a user
        /// </summary>
        /// <param name="id">The user's ID</param>
        /// <returns>List of roles</returns>
        [HttpGet("{id:guid}/roles")]
        [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetUserRoles(Guid id)
        {
            _logger.LogInformation("Getting roles for user {UserId}", id);

            try
            {
                var query = new GetUserRolesQuery { UserId = id };
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not found", id);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user roles");
                return BadRequest(new { Message = "An error occurred while retrieving user roles" });
            }
        }

        // ====================================
        // COMMANDS (Write Operations)
        // ====================================

        /// <summary>
        /// Updates the current user's profile information
        /// </summary>
        /// <param name="dto">Updated profile information</param>
        /// <returns>No content on success</returns>
        [HttpPut("me/profile")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            // Extract user ID from JWT claims
            var userIdClaim = User?.FindFirst("sub")?.Value ?? User?.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("No valid user ID found in claims");
                return Unauthorized(new { Message = "User not authenticated" });
            }

            _logger.LogInformation("Updating profile for user {UserId}", userId);

            try
            {
                var command = new UpdateProfileCommand
                {
                    UserId = userId,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.PhoneNumber
                };

                await _mediator.Send(command);

                _logger.LogInformation("Profile updated successfully for user {UserId}", userId);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not found", userId);
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Profile update failed for user {UserId}", userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating profile");
                return BadRequest(new { Message = "An error occurred while updating profile" });
            }
        }

        /// <summary>
        /// Changes the current user's password
        /// </summary>
        /// <param name="dto">Password change information</param>
        /// <returns>No content on success</returns>
        [HttpPut("me/password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            // Extract user ID from JWT claims
            var userIdClaim = User?.FindFirst("sub")?.Value ?? User?.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("No valid user ID found in claims");
                return Unauthorized(new { Message = "User not authenticated" });
            }

            _logger.LogInformation("Changing password for user {UserId}", userId);

            try
            {
                var command = new ChangePasswordCommand
                {
                    UserId = userId,
                    CurrentPassword = dto.CurrentPassword,
                    NewPassword = dto.NewPassword
                };

                await _mediator.Send(command);

                _logger.LogInformation("Password changed successfully for user {UserId}", userId);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not found", userId);
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Password change failed for user {UserId}", userId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Password change failed for user {UserId}", userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password");
                return BadRequest(new { Message = "An error occurred while changing password" });
            }
        }
    }
}
