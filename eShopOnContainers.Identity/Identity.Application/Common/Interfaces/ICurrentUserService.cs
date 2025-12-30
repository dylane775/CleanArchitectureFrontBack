namespace Identity.Application.Common.Interfaces
{
    /// <summary>
    /// Service for accessing current authenticated user information
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the current user's ID
        /// </summary>
        Guid? UserId { get; }

        /// <summary>
        /// Gets the current user's email
        /// </summary>
        string? Email { get; }

        /// <summary>
        /// Gets the current user's roles
        /// </summary>
        IEnumerable<string> Roles { get; }

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        /// <param name="role">Role name to check</param>
        /// <returns>True if user has the role, false otherwise</returns>
        bool IsInRole(string role);
    }
}
