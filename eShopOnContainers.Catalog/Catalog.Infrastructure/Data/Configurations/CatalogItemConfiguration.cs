using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Catalog.Domain.Entities;

namespace Catalog.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Configuration Fluent API pour l'entité CatalogItem
    /// Définit comment mapper CatalogItem vers la table SQL
    /// </summary>
    public class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItem>
    {
        public void Configure(EntityTypeBuilder<CatalogItem> builder)
        {
            // ====================================
            // TABLE
            // ====================================
            builder.ToTable("CatalogItems");

            // ====================================
            // CLÉ PRIMAIRE
            // ====================================
            builder.HasKey(x => x.Id);

            // ====================================
            // PROPRIÉTÉS MÉTIER
            // ====================================
            
            // Name - Requis, max 100 caractères
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Description - Optionnel, max 500 caractères
            builder.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired(false); // ✅ Explicite: optionnel

            // Price - Type décimal avec précision
            builder.Property(x => x.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            // PictureFileName - Requis
            builder.Property(x => x.PictureFileName)
                .IsRequired()
                .HasMaxLength(200);

            // PictureUri - Optionnel
            builder.Property(x => x.PictureUri)
                .HasMaxLength(500)
                .IsRequired(false); // ✅ Explicite: optionnel

            // ====================================
            // GESTION DU STOCK
            // ====================================
            
            builder.Property(x => x.AvailableStock)
                .IsRequired();

            builder.Property(x => x.RestockThreshold)
                .IsRequired();

            builder.Property(x => x.MaxStockThreshold)
                .IsRequired();

            builder.Property(x => x.OnReorder)
                .IsRequired();

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
                .IsRequired(false); // ✅ Explicite: optionnel

            builder.Property(x => x.ModifiedBy)
                .HasMaxLength(50)
                .IsRequired(false); // ✅ CORRECTION: rendre optionnel

            // Suppression logique
            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.DeletedAt)
                .IsRequired(false); // ✅ Explicite: optionnel

            builder.Property(x => x.DeletedBy)
                .HasMaxLength(50)
                .IsRequired(false); // ✅ CORRECTION: rendre optionnel

            // ====================================
            // RELATIONS (FOREIGN KEYS)
            // ====================================
            
            // Relation avec CatalogType (Many-to-One)
            builder.HasOne(x => x.CatalogType)
                .WithMany()  // Un CatalogType peut avoir plusieurs CatalogItems
                .HasForeignKey(x => x.CatalogTypeId)
                .OnDelete(DeleteBehavior.Restrict);  // Ne pas supprimer en cascade

            // Relation avec CatalogBrand (Many-to-One)
            builder.HasOne(x => x.CatalogBrand)
                .WithMany()  // Une CatalogBrand peut avoir plusieurs CatalogItems
                .HasForeignKey(x => x.CatalogBrandId)
                .OnDelete(DeleteBehavior.Restrict);  // Ne pas supprimer en cascade

            // ====================================
            // INDEX (pour les performances)
            // ====================================
            
            // Index sur CatalogTypeId (requêtes fréquentes par type)
            builder.HasIndex(x => x.CatalogTypeId)
                .HasDatabaseName("IX_CatalogItems_CatalogTypeId");

            // Index sur CatalogBrandId (requêtes fréquentes par marque)
            builder.HasIndex(x => x.CatalogBrandId)
                .HasDatabaseName("IX_CatalogItems_CatalogBrandId");

            // Index sur Name (recherche par nom)
            builder.HasIndex(x => x.Name)
                .HasDatabaseName("IX_CatalogItems_Name");

            // Index sur IsDeleted (pour les soft deletes)
            builder.HasIndex(x => x.IsDeleted)
                .HasDatabaseName("IX_CatalogItems_IsDeleted");

            // ====================================
            // QUERY FILTER (Soft Delete automatique)
            // ====================================
            
            // Toutes les requêtes ignoreront automatiquement les entités supprimées
            builder.HasQueryFilter(x => !x.IsDeleted);

            // ====================================
            // IGNORER LES PROPRIÉTÉS (non mappées)
            // ====================================
            
            // DomainEvents n'est PAS mappé en base (c'est une collection en mémoire)
            builder.Ignore(x => x.DomainEvents);
        }
    }
}