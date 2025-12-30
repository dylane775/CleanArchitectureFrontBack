using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ordering.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ordering.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // ====================================
            // TABLE
            // ====================================
            builder.ToTable("Orders");

            // ====================================
            // CLÉ PRIMAIRE
            // ====================================
            builder.HasKey(x => x.Id);

            // ====================================
            // PROPRIÉTÉS MÉTIER
            // ====================================
            builder.Property(x => x.CustomerId)
                .IsRequired();

            builder.Property(x => x.OrderDate)
                .IsRequired();

            builder.Property(x => x.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.OrderStatus)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ShippingAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.BillingAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.PaymentMethod)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.CustomerEmail)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.CustomerPhone)
                .HasMaxLength(50);

            builder.Property(x => x.DeliveryDate)
                .IsRequired(false);

            // ====================================
            // RELATIONS
            // ====================================
            builder.HasMany(x => x.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ====================================
            // INDEX
            // ====================================
            builder.HasIndex(x => x.CustomerId);
            builder.HasIndex(x => x.OrderStatus);
            builder.HasIndex(x => x.OrderDate);

            // Global Query Filter (Soft Delete)
            builder.HasQueryFilter(x => !x.IsDeleted);

            // ====================================
            // PROPRIÉTÉS D'AUDIT
            // ====================================

            // Création (OBLIGATOIRES)
            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .IsRequired()
                .HasMaxLength(50);

            // Modification (OPTIONNELS - null jusqu'à première modification)
            builder.Property(x => x.ModifiedAt)
                .IsRequired(false); // Explicite: optionnel

            builder.Property(x => x.ModifiedBy)
                .HasMaxLength(50)
                .IsRequired(false); // Explicite: optionnel

            // Suppression logique
            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.DeletedAt)
                .IsRequired(false); // Explicite: optionnel
        }
    }
}