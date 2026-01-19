using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Catalog.Domain.Entities;
using System.Reflection;
using Catalog.Domain.Common;

/// <summary>
/// DbContext pour le microservice Catalog
/// Gère la connexion à SQL Server et le mapping des entités
/// </summary>

namespace Catalog.Infrastructure.Data
{
     // ====================================
        // DBSETS (Collections d'entités)
        // ====================================
        
        
    public class CatalogContext : DbContext
    {
        /// <summary>
        /// Collection des produits du catalogue
        /// Correspond à la table "CatalogItems" en base
        /// </summary>
        
        public DbSet<CatalogItem> CatalogItems { get; set; }

        /// <summary>
        /// Collection des types/catégories
        /// Correspond à la table "CatalogTypes" en base
        /// </summary>
        
        public DbSet<CatalogType> CatalogType { get; set; }

        /// <summary>
        /// Collection des marques
        /// Correspond à la table "CatalogBrands" en base
        /// </summary>

        public DbSet<CatalogBrand> CatalogBrands { get; set; }

        /// <summary>
        /// Collection des avis clients
        /// Correspond à la table "ProductReviews" en base
        /// </summary>
        public DbSet<ProductReview> ProductReviews { get; set; }

          // ====================================
        // CONSTRUCTEURS
        // ====================================
        
        /// <summary>
        /// Constructeur avec options (injecté par DI)
        /// </summary>
        
        public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
        {
            
        }
         protected CatalogContext()
        {
        }

        // ====================================
        // CONFIGURATION DU MODÈLE
        // ====================================
        
        /// <summary>
        /// Configure le modèle de données
        /// Applique automatiquement toutes les configurations (Fluent API)
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Applique automatiquement TOUTES les configurations IEntityTypeConfiguration
            // dans l'assembly actuel (Catalog.Infrastructure)
            // Trouve : CatalogItemConfiguration, CatalogTypeConfiguration, CatalogBrandConfiguration
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Alternative manuelle (si on ne veut pas l'auto-scan) :
            // modelBuilder.ApplyConfiguration(new CatalogItemConfiguration());
            // modelBuilder.ApplyConfiguration(new CatalogTypeConfiguration());
            // modelBuilder.ApplyConfiguration(new CatalogBrandConfiguration());
        }

        /// <summary>
        /// Override SaveChanges pour gérer l'audit automatiquement
        /// Remplit CreatedAt, ModifiedAt, etc. avant de sauvegarder
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Avant de sauvegarder, on applique l'audit
            ApplyAuditInformation();

            // Puis on sauvegarde normalement
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Applique automatiquement les informations d'audit
        /// (CreatedAt, ModifiedAt, CreatedBy, ModifiedBy)
        /// </summary>
        private void ApplyAuditInformation()
        {
            var entries = ChangeTracker.Entries<Entity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // Nouvelle entité : définir CreatedAt et CreatedBy
                        entry.Entity.SetCreated("system"); // TODO: Récupérer l'utilisateur actuel
                        break;

                    case EntityState.Modified:
                        // Entité modifiée : définir ModifiedAt et ModifiedBy
                        entry.Entity.SetModified("system"); // TODO: Récupérer l'utilisateur actuel
                        break;

                    case EntityState.Deleted:
                        // Soft Delete : au lieu de supprimer, marquer comme supprimé
                        entry.State = EntityState.Modified;
                        entry.Entity.SetDeleted("system"); // TODO: Récupérer l'utilisateur actuel
                        break;
                }
            }
        }

    }
}