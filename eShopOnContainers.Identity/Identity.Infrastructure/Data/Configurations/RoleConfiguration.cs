using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for Role entity
    /// </summary>
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            // ====================================
            // TABLE
            // ====================================
            builder.ToTable("Roles");

            // ====================================
            // PRIMARY KEY
            // ====================================
            builder.HasKey(x => x.Id);

            // ====================================
            // BUSINESS PROPERTIES
            // ====================================
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Permissions)
                .IsRequired()
                .HasMaxLength(2000)
                .HasDefaultValue("[]");

            // ====================================
            // RELATIONSHIPS
            // ====================================
            // Many-to-Many relationship is configured in UserConfiguration

            // ====================================
            // INDEXES
            // ====================================
            builder.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("IX_Roles_Name");

            // ====================================
            // GLOBAL QUERY FILTER (Soft Delete)
            // ====================================
            builder.HasQueryFilter(x => !x.IsDeleted);

            // ====================================
            // AUDIT PROPERTIES
            // ====================================

            // Creation (REQUIRED)
            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .IsRequired()
                .HasMaxLength(50);

            // Modification (OPTIONAL)
            builder.Property(x => x.ModifiedAt)
                .IsRequired(false);

            builder.Property(x => x.ModifiedBy)
                .HasMaxLength(50)
                .IsRequired(false);

            // Soft Delete
            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.DeletedAt)
                .IsRequired(false);

            builder.Property(x => x.DeletedBy)
                .HasMaxLength(50)
                .IsRequired(false);
        }
    }
}
