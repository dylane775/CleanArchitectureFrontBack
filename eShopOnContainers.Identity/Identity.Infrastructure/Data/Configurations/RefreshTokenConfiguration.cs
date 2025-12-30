using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for RefreshToken entity
    /// </summary>
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            // ====================================
            // TABLE
            // ====================================
            builder.ToTable("RefreshTokens");

            // ====================================
            // PRIMARY KEY
            // ====================================
            builder.HasKey(x => x.Id);

            // ====================================
            // BUSINESS PROPERTIES
            // ====================================
            builder.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.Property(x => x.CreatedByIp)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.RevokedAt)
                .IsRequired(false);

            builder.Property(x => x.RevokedByIp)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(x => x.ReplacedByToken)
                .HasMaxLength(500)
                .IsRequired(false);

            // ====================================
            // RELATIONSHIPS
            // ====================================
            // Many-to-One: RefreshToken -> User
            // Configured in UserConfiguration with HasMany

            // ====================================
            // INDEXES
            // ====================================
            builder.HasIndex(x => x.Token)
                .IsUnique()
                .HasDatabaseName("IX_RefreshTokens_Token");

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_RefreshTokens_UserId");

            builder.HasIndex(x => x.ExpiresAt)
                .HasDatabaseName("IX_RefreshTokens_ExpiresAt");

            builder.HasIndex(x => x.RevokedAt)
                .HasDatabaseName("IX_RefreshTokens_RevokedAt");

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
