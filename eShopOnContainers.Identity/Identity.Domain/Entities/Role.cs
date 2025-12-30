using System;
using System.Collections.Generic;
using System.Linq;
using Identity.Domain.Common;
using Identity.Domain.Exceptions;

namespace Identity.Domain.Entities
{
    /// <summary>
    /// Represents a role in the identity system
    /// </summary>
    public class Role : BaseEntity
    {
        // ====== PROPERTIES ======
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string Permissions { get; private set; } = string.Empty; // JSON serialized permissions

        // Navigation property
        private readonly List<User> _users = new();
        public IReadOnlyCollection<User> Users => _users.AsReadOnly();

        // ====== COMMON ROLE CONSTANTS ======
        public const string Admin = "Admin";
        public const string Customer = "Customer";
        public const string Manager = "Manager";

        // ====== CONSTRUCTORS ======
        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Role()
        {
        }

        /// <summary>
        /// Creates a new role
        /// </summary>
        /// <param name="name">Unique role name</param>
        /// <param name="description">Role description</param>
        /// <param name="permissions">JSON serialized permissions (optional)</param>
        public Role(string name, string description, string? permissions = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new IdentityDomainException("Role name cannot be empty");

            if (string.IsNullOrWhiteSpace(description))
                throw new IdentityDomainException("Role description cannot be empty");

            Name = name.Trim();
            Description = description.Trim();
            Permissions = permissions ?? "[]"; // Default to empty JSON array
        }

        // ====== BUSINESS METHODS ======

        /// <summary>
        /// Updates the role's information
        /// </summary>
        /// <param name="name">New role name</param>
        /// <param name="description">New role description</param>
        public void UpdateRole(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new IdentityDomainException("Role name cannot be empty");

            if (string.IsNullOrWhiteSpace(description))
                throw new IdentityDomainException("Role description cannot be empty");

            Name = name.Trim();
            Description = description.Trim();

            UpdateAuditInfo("system");
        }

        /// <summary>
        /// Adds a permission to the role
        /// </summary>
        /// <param name="permission">Permission to add</param>
        public void AddPermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                throw new IdentityDomainException("Permission cannot be empty");

            var permissionsList = DeserializePermissions();

            if (permissionsList.Contains(permission))
                throw new IdentityDomainException($"Permission {permission} already exists in role {Name}");

            permissionsList.Add(permission);
            Permissions = SerializePermissions(permissionsList);

            UpdateAuditInfo("system");
        }

        /// <summary>
        /// Removes a permission from the role
        /// </summary>
        /// <param name="permission">Permission to remove</param>
        public void RemovePermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                throw new IdentityDomainException("Permission cannot be empty");

            var permissionsList = DeserializePermissions();

            if (!permissionsList.Contains(permission))
                throw new IdentityDomainException($"Permission {permission} does not exist in role {Name}");

            permissionsList.Remove(permission);
            Permissions = SerializePermissions(permissionsList);

            UpdateAuditInfo("system");
        }

        /// <summary>
        /// Checks if the role has a specific permission
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <returns>True if role has the permission, false otherwise</returns>
        public bool HasPermission(string permission)
        {
            var permissionsList = DeserializePermissions();
            return permissionsList.Contains(permission);
        }

        /// <summary>
        /// Gets all permissions for the role
        /// </summary>
        /// <returns>List of permissions</returns>
        public List<string> GetPermissions()
        {
            return DeserializePermissions();
        }

        /// <summary>
        /// Sets all permissions for the role
        /// </summary>
        /// <param name="permissions">List of permissions to set</param>
        public void SetPermissions(List<string> permissions)
        {
            if (permissions == null)
                throw new IdentityDomainException("Permissions list cannot be null");

            Permissions = SerializePermissions(permissions);
            UpdateAuditInfo("system");
        }

        /// <summary>
        /// Checks if this is a system role (Admin, Customer, Manager)
        /// </summary>
        /// <returns>True if system role, false otherwise</returns>
        public bool IsSystemRole()
        {
            return Name == Admin || Name == Customer || Name == Manager;
        }

        // ====== HELPER METHODS ======

        /// <summary>
        /// Deserializes the JSON permissions string to a list
        /// </summary>
        private List<string> DeserializePermissions()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Permissions) || Permissions == "[]")
                    return new List<string>();

                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Permissions) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Serializes a list of permissions to JSON string
        /// </summary>
        private string SerializePermissions(List<string> permissions)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(permissions);
            }
            catch
            {
                return "[]";
            }
        }
    }
}
