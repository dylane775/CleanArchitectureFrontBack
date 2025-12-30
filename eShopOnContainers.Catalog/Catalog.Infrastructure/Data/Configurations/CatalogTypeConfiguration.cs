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
    /// Configuration Fluent API pour l'entité CatalogType
    /// </summary>
    public class CatalogTypeConfiguration : IEntityTypeConfiguration<CatalogType>
    {
        public void Configure(EntityTypeBuilder<CatalogType> builder)
        {
            // ====================================
            // TABLE
            // ====================================
            builder.ToTable("CatalogTypes");

            // ====================================
            // CLÉ PRIMAIRE
            // ====================================
            builder.HasKey(x => x.Id);

            // ====================================
            // PROPRIÉTÉS MÉTIER
            // ====================================
            builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(100);

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
            // QUERY FILTER (Soft Delete automatique)
            // ====================================
            builder.HasQueryFilter(x => !x.IsDeleted);

            // ====================================
            // IGNORER PROPRIÉTÉS NON MAPPÉES
            // ====================================
            builder.Ignore(x => x.DomainEvents);

            // ====================================
            // INDEX
            // ====================================
            builder.HasIndex(x => x.Type)
                .IsUnique()
                .HasDatabaseName("IX_CatalogTypes_Type");
        }
    }
}