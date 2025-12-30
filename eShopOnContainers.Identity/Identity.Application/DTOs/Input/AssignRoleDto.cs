namespace Identity.Application.DTOs.Input
{
    /// <summary>
    /// Data transfer object for assigning a role to a user
    /// </summary>
    public class AssignRoleDto
    {
        /// <summary>
        /// User ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Role name to assign
        /// </summary>
        public string RoleName { get; set; } = string.Empty;
    }
}
