using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Basket.Domain.Entities;

namespace Basket.Infrastructure.Data.Configurations
{
    public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
    {
        public void Configure(EntityTypeBuilder<WishlistItem> builder)
        {
            builder.ToTable("WishlistItems");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(w => w.CatalogItemId)
                .IsRequired();

            builder.Property(w => w.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(w => w.Price)
                .HasPrecision(18, 2);

            builder.Property(w => w.PictureUrl)
                .HasMaxLength(500);

            builder.Property(w => w.BrandName)
                .HasMaxLength(100);

            builder.Property(w => w.CategoryName)
                .HasMaxLength(100);

            builder.Property(w => w.AddedAt)
                .IsRequired();

            // Audit fields - allows nulls for wishlist items
            builder.Property(w => w.CreatedBy)
                .HasMaxLength(450)
                .IsRequired(false);

            builder.Property(w => w.ModifiedBy)
                .HasMaxLength(450)
                .IsRequired(false);

            builder.Property(w => w.DeletedBy)
                .HasMaxLength(450)
                .IsRequired(false);

            // Index pour les requêtes par utilisateur
            builder.HasIndex(w => w.UserId);

            // Index unique pour éviter les doublons (un utilisateur ne peut pas avoir le même produit deux fois)
            builder.HasIndex(w => new { w.UserId, w.CatalogItemId })
                .IsUnique();
        }
    }
}
