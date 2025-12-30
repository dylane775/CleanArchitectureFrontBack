using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Basket.Domain.Entities;

namespace Basket.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Configuration Fluent API pour l'entité CustomerBasket
    /// Définit comment mapper CustomerBasket vers la table SQL
    /// </summary>
    public class CustomerBasketConfiguration : IEntityTypeConfiguration<CustomerBasket>
    {
        public void Configure(EntityTypeBuilder<CustomerBasket> builder)
        {
            // ====================================
            // TABLE
            // ====================================
            builder.ToTable("CustomerBaskets");

            // ====================================
            // CLÉ PRIMAIRE
            // ====================================
            builder.HasKey(b => b.Id);

            // ====================================
            // PROPRIÉTÉS MÉTIER
            // ====================================

            // CustomerId - Requis, max 100 caractères
            builder.Property(b => b.CustomerId)
                .IsRequired()
                .HasMaxLength(100);

            // ====================================
            // PROPRIÉTÉS D'AUDIT
            // ====================================

            // Création (OBLIGATOIRES)
            builder.Property(b => b.CreatedAt)
                .IsRequired();

            builder.Property(b => b.CreatedBy)
                .IsRequired()
                .HasMaxLength(50);

            // Modification (OPTIONNELS - null jusqu'à première modification)
            builder.Property(b => b.ModifiedAt)
                .IsRequired(false);

            builder.Property(b => b.ModifiedBy)
                .HasMaxLength(50)
                .IsRequired(false);

            // Suppression logique
            builder.Property(b => b.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(b => b.DeletedAt)
                .IsRequired(false);

            builder.Property(b => b.DeletedBy)
                .HasMaxLength(50)
                .IsRequired(false);

            // ====================================
            // RELATIONS (FOREIGN KEYS)
            // ====================================

            // Relation CustomerBasket 1 ---> * BasketItem
            // ✅ Utilise la propriété publique Items (EF Core détecte les changements)
            builder.HasMany(b => b.Items)
                .WithOne(i => i.CustomerBasket)
                .HasForeignKey(i => i.CustomerBasketId)
                .OnDelete(DeleteBehavior.Cascade);

            // ====================================
            // INDEX (pour les performances)
            // ====================================

            // Index sur CustomerId (requêtes fréquentes par client)
            builder.HasIndex(b => b.CustomerId)
                .HasDatabaseName("IX_CustomerBaskets_CustomerId");

            // Index sur IsDeleted (pour les soft deletes)
            builder.HasIndex(b => b.IsDeleted)
                .HasDatabaseName("IX_CustomerBaskets_IsDeleted");

            // ====================================
            // QUERY FILTER (Soft Delete automatique)
            // ====================================

            // Toutes les requêtes ignoreront automatiquement les entités supprimées
            builder.HasQueryFilter(b => !b.IsDeleted);

            // ====================================
            // IGNORER LES PROPRIÉTÉS (non mappées)
            // ====================================

            // DomainEvents n'est PAS mappé en base (c'est une collection en mémoire)
            builder.Ignore(b => b.DomainEvents);
        }
    }
}