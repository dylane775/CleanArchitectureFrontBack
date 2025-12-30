namespace Identity.Application.DTOs.Output
{
    /// <summary>
    /// Data transfer object for Role entity
    /// </summary>
    public class RoleDto
    {
        /// <summary>
        /// Role's unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Role name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Role description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// List of permissions associated with this role
        /// </summary>
        public List<string> Permissions { get; set; } = new();
    }
}
