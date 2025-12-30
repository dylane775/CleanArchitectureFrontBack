using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for User entity
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // ====================================
            // TABLE
            // ====================================
            builder.ToTable("Users");

            // ====================================
            // PRIMARY KEY
            // ====================================
            builder.HasKey(x => x.Id);

            // ====================================
            // BUSINESS PROPERTIES
            // ====================================
            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.PhoneNumber)
                .HasMaxLength(50);

            builder.Property(x => x.IsEmailConfirmed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.LastLoginAt)
                .IsRequired(false);

            builder.Property(x => x.EmailConfirmationToken)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.EmailConfirmationTokenExpiresAt)
                .IsRequired(false);

            builder.Property(x => x.PasswordResetToken)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.PasswordResetTokenExpiresAt)
                .IsRequired(false);

            // ====================================
            // RELATIONSHIPS
            // ====================================

            // One-to-Many: User -> RefreshTokens
            builder.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-Many: User <-> Roles
            builder.HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRoles",
                    j => j.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("UserRoles");
                    });

            // ====================================
            // INDEXES
            // ====================================
            builder.HasIndex(x => x.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(x => x.IsActive)
                .HasDatabaseName("IX_Users_IsActive");

            builder.HasIndex(x => x.IsEmailConfirmed)
                .HasDatabaseName("IX_Users_IsEmailConfirmed");

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
