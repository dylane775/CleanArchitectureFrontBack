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
    /// Configuration Fluent API pour l'entité BasketItem
    /// Définit comment mapper BasketItem vers la table SQL
    /// </summary>
    public class BasketItemConfiguration : IEntityTypeConfiguration<BasketItem>
    {
        public void Configure(EntityTypeBuilder<BasketItem> builder)
        {
            // ====================================
            // TABLE
            // ====================================
            builder.ToTable("BasketItems");

            // ====================================
            // CLÉ PRIMAIRE
            // ====================================
            builder.HasKey(bi => bi.Id);

            // ====================================
            // PROPRIÉTÉS MÉTIER
            // ====================================
            
            // CatalogItemId - Requis (référence vers le produit du Catalog)
            builder.Property(bi => bi.CatalogItemId)
                .IsRequired();

            // ProductName - Requis, max 200 caractères
            builder.Property(bi => bi.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            // UnitPrice - Type décimal avec précision
            builder.Property(bi => bi.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            // Quantity - Requis
            builder.Property(bi => bi.Quantity)
                .IsRequired();

            // PictureUrl - Optionnel, max 500 caractères
            builder.Property(bi => bi.PictureUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            // CustomerBasketId - Foreign Key vers CustomerBasket
            builder.Property(bi => bi.CustomerBasketId)
                .IsRequired();

            // ====================================
            // PROPRIÉTÉS D'AUDIT
            // ====================================
            
            // Création (OBLIGATOIRES)
            builder.Property(bi => bi.CreatedAt)
                .IsRequired();

            builder.Property(bi => bi.CreatedBy)
                .IsRequired()
                .HasMaxLength(50);

            // Modification (OPTIONNELS - null jusqu'à première modification)
            builder.Property(bi => bi.ModifiedAt)
                .IsRequired(false);

            builder.Property(bi => bi.ModifiedBy)
                .HasMaxLength(50)
                .IsRequired(false);

            // Suppression logique
            builder.Property(bi => bi.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(bi => bi.DeletedAt)
                .IsRequired(false);

            builder.Property(bi => bi.DeletedBy)
                .HasMaxLength(50)
                .IsRequired(false);

            // ====================================
            // INDEX (pour les performances)
            // ====================================
            
            // Index sur CatalogItemId (requêtes pour voir quels paniers contiennent un produit)
            builder.HasIndex(bi => bi.CatalogItemId)
                .HasDatabaseName("IX_BasketItems_CatalogItemId");

            // Index sur CustomerBasketId (déjà créé par EF pour la FK, mais on le nomme)
            builder.HasIndex(bi => bi.CustomerBasketId)
                .HasDatabaseName("IX_BasketItems_CustomerBasketId");

            // Index composé pour recherche rapide (quel item dans quel panier)
            builder.HasIndex(bi => new { bi.CustomerBasketId, bi.CatalogItemId })
                .HasDatabaseName("IX_BasketItems_CustomerBasketId_CatalogItemId");

            // Index sur IsDeleted (pour les soft deletes)
            builder.HasIndex(bi => bi.IsDeleted)
                .HasDatabaseName("IX_BasketItems_IsDeleted");

            // ====================================
            // QUERY FILTER (Soft Delete automatique)
            // ====================================
            
            // Toutes les requêtes ignoreront automatiquement les entités supprimées
            builder.HasQueryFilter(bi => !bi.IsDeleted);

            // ====================================
            // IGNORER LES PROPRIÉTÉS (non mappées)
            // ====================================
            
            // DomainEvents n'est PAS mappé en base (c'est une collection en mémoire)
            builder.Ignore(bi => bi.DomainEvents);
        }
    }
}