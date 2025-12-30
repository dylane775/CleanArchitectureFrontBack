using Identity.Application.Commands.AssignRole;
using Identity.Application.Commands.RemoveRole;
using Identity.Application.DTOs.Input;
using Identity.Application.DTOs.Output;
using Identity.Application.Queries.GetRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    /// <summary>
    /// Controller for role management operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IMediator mediator, ILogger<RolesController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all available roles
        /// </summary>
        /// <returns>List of roles</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
        {
            try
            {
                var query = new GetRolesQuery();
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching roles");
                return StatusCode(500, new { Message = "An error occurred while fetching roles" });
            }
        }

        /// <summary>
        /// Assign a role to a user
        /// </summary>
        /// <param name="dto">Role assignment data</param>
        /// <returns>Success status</returns>
        [HttpPost("assign")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> AssignRole([FromBody] AssignRoleDto dto)
        {
            try
            {
                var command = new AssignRoleCommand
                {
                    UserId = dto.UserId,
                    RoleName = dto.RoleName
                };

                var result = await _mediator.Send(command);

                _logger.LogInformation("Role {RoleName} assigned to user {UserId}", dto.RoleName, dto.UserId);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to assign role");
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning role");
                return StatusCode(500, new { Message = "An error occurred while assigning the role" });
            }
        }

        /// <summary>
        /// Remove a role from a user
        /// </summary>
        /// <param name="dto">Role removal data</param>
        /// <returns>Success status</returns>
        [HttpPost("remove")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> RemoveRole([FromBody] AssignRoleDto dto)
        {
            try
            {
                var command = new RemoveRoleCommand
                {
                    UserId = dto.UserId,
                    RoleName = dto.RoleName
                };

                var result = await _mediator.Send(command);

                _logger.LogInformation("Role {RoleName} removed from user {UserId}", dto.RoleName, dto.UserId);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to remove role");
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing role");
                return StatusCode(500, new { Message = "An error occurred while removing the role" });
            }
        }
    }
}
