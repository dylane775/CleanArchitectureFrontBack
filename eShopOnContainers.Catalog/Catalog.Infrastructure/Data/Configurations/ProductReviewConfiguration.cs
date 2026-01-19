using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Catalog.Domain.Entities;

namespace Catalog.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Configuration Fluent API pour l'entité ProductReview
    /// Définit comment mapper ProductReview vers la table SQL
    /// </summary>
    public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            // ====================================
            // TABLE
            // ====================================
            builder.ToTable("ProductReviews");

            // ====================================
            // CLÉ PRIMAIRE
            // ====================================
            builder.HasKey(x => x.Id);

            // ====================================
            // PROPRIÉTÉS MÉTIER
            // ====================================

            builder.Property(x => x.CatalogItemId)
                .IsRequired();

            builder.Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.UserDisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Rating)
                .IsRequired();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Comment)
                .HasMaxLength(2000)
                .IsRequired(false);

            builder.Property(x => x.IsVerifiedPurchase)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.HelpfulCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.TotalVotes)
                .IsRequired()
                .HasDefaultValue(0);

            // ====================================
            // PROPRIÉTÉS D'AUDIT
            // ====================================

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ModifiedAt)
                .IsRequired(false);

            builder.Property(x => x.ModifiedBy)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.DeletedAt)
                .IsRequired(false);

            builder.Property(x => x.DeletedBy)
                .HasMaxLength(50)
                .IsRequired(false);

            // ====================================
            // RELATIONS (FOREIGN KEYS)
            // ====================================

            builder.HasOne(x => x.CatalogItem)
                .WithMany()
                .HasForeignKey(x => x.CatalogItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // ====================================
            // INDEX
            // ====================================

            builder.HasIndex(x => x.CatalogItemId)
                .HasDatabaseName("IX_ProductReviews_CatalogItemId");

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_ProductReviews_UserId");

            builder.HasIndex(x => new { x.CatalogItemId, x.UserId })
                .IsUnique()
                .HasDatabaseName("IX_ProductReviews_CatalogItemId_UserId");

            builder.HasIndex(x => x.Rating)
                .HasDatabaseName("IX_ProductReviews_Rating");

            builder.HasIndex(x => x.IsDeleted)
                .HasDatabaseName("IX_ProductReviews_IsDeleted");

            // ====================================
            // QUERY FILTER (Soft Delete)
            // ====================================

            builder.HasQueryFilter(x => !x.IsDeleted);

            // ====================================
            // IGNORER LES PROPRIÉTÉS
            // ====================================

            builder.Ignore(x => x.DomainEvents);
        }
    }
}
