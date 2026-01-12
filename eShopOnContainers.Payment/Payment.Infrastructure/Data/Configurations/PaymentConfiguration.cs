using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Enums;

namespace Payment.Infrastructure.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Domain.Entities.Payment>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Payment> builder)
        {
            // ====================================
            // TABLE
            // ====================================
            builder.ToTable("Payments");

            // ====================================
            // CLÉ PRIMAIRE
            // ====================================
            builder.HasKey(x => x.Id);

            // ====================================
            // PROPRIÉTÉS MÉTIER
            // ====================================
            builder.Property(x => x.OrderId)
                .IsRequired();

            builder.Property(x => x.CustomerId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.Provider)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.TransactionId)
                .HasMaxLength(200);

            builder.Property(x => x.PaymentReference)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.CustomerEmail)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.CustomerPhone)
                .HasMaxLength(20);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.CallbackUrl)
                .HasMaxLength(500);

            builder.Property(x => x.ReturnUrl)
                .HasMaxLength(500);

            builder.Property(x => x.FailureReason)
                .HasMaxLength(500);

            builder.Property(x => x.CompletedAt)
                .IsRequired(false);

            builder.Property(x => x.FailedAt)
                .IsRequired(false);

            builder.Property(x => x.RefundedAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(x => x.RefundedAt)
                .IsRequired(false);

            // ====================================
            // INDEX
            // ====================================
            builder.HasIndex(x => x.OrderId)
                .IsUnique();

            builder.HasIndex(x => x.CustomerId);

            builder.HasIndex(x => x.Status);

            builder.HasIndex(x => x.Provider);

            builder.HasIndex(x => x.TransactionId);

            builder.HasIndex(x => x.PaymentReference)
                .IsUnique();

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

            // Modification (OPTIONNELS)
            builder.Property(x => x.ModifiedAt)
                .IsRequired(false);

            builder.Property(x => x.ModifiedBy)
                .HasMaxLength(50)
                .IsRequired(false);

            // Suppression logique
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
