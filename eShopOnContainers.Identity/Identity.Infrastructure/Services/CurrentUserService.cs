using System.Security.Claims;
using Identity.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Identity.Infrastructure.Services
{
    /// <summary>
    /// Service for accessing current authenticated user information from HttpContext
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Gets the current user's ID from JWT claims
        /// </summary>
        public Guid? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the current user's email from JWT claims
        /// </summary>
        public string? Email
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            }
        }

        /// <summary>
        /// Gets the current user's roles from JWT claims
        /// </summary>
        public IEnumerable<string> Roles
        {
            get
            {
                var roleClaims = _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role);

                if (roleClaims == null)
                {
                    return Enumerable.Empty<string>();
                }

                return roleClaims.Select(c => c.Value).ToList();
            }
        }

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
            }
        }

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        /// <param name="role">Role name to check</param>
        /// <returns>True if user has the role, false otherwise</returns>
        public bool IsInRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return false;

            return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        }
    }
}
