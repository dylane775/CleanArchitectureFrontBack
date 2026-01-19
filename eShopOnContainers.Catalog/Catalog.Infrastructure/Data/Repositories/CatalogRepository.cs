using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;

namespace Catalog.Infrastructure.Data.Repositories
{

    /// <summary>
    /// Implémentation du repository pour CatalogItem
    /// Gère toutes les opérations de données pour les produits du catalogue
    /// </summary>

    public class CatalogRepository : ICatalogRepository
    {
       private readonly CatalogContext _context;

        public CatalogRepository(CatalogContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ====================================
        // LECTURE (Queries)
        // ====================================

        /// <summary>
        /// Récupère un produit par son ID avec ses relations
        /// </summary>
        public async Task<CatalogItem?> GetByIdAsync(Guid id)
        {
            return await _context.CatalogItems
                .Include(x => x.CatalogType)   // Charge la relation CatalogType
                .Include(x => x.CatalogBrand)  // Charge la relation CatalogBrand
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Récupère tous les produits avec leurs relations
        /// </summary>
        public async Task<IEnumerable<CatalogItem>> GetAllAsync()
        {
            return await _context.CatalogItems
                .Include(x => x.CatalogType)
                .Include(x => x.CatalogBrand)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les produits de manière paginée
        /// </summary>
        public async Task<PaginatedItems<CatalogItem>> GetPagedAsync(int pageIndex, int pageSize)
        {
            // Compter le nombre total d'éléments
            var totalItems = await _context.CatalogItems.LongCountAsync();

            // Récupérer les éléments de la page demandée
            // PageIndex is 1-based, so subtract 1 for Skip calculation
            var items = await _context.CatalogItems
                .Include(x => x.CatalogType)
                .Include(x => x.CatalogBrand)
                .OrderBy(x => x.Name)  // Tri par nom
                .Skip((pageIndex - 1) * pageSize)  // Sauter les pages précédentes (1-based pagination)
                .Take(pageSize)  // Prendre uniquement pageSize éléments
                .ToListAsync();

            return new PaginatedItems<CatalogItem>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Count = totalItems,
                Data = items
            };
        }

        /// <summary>
        /// Récupère tous les produits d'une marque spécifique
        /// </summary>
        public async Task<IEnumerable<CatalogItem>> GetByBrandAsync(Guid brandId)
        {
            return await _context.CatalogItems
                .Include(x => x.CatalogType)
                .Include(x => x.CatalogBrand)
                .Where(x => x.CatalogBrandId == brandId)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère tous les produits d'un type spécifique
        /// </summary>
        public async Task<IEnumerable<CatalogItem>> GetByTypeAsync(Guid typeId)
        {
            return await _context.CatalogItems
                .Include(x => x.CatalogType)
                .Include(x => x.CatalogBrand)
                .Where(x => x.CatalogTypeId == typeId)
                .ToListAsync();
        }

        /// <summary>
        /// Recherche les produits par nom, description ou marque (pour auto-complétion)
        /// </summary>
        public async Task<IEnumerable<CatalogItem>> SearchAsync(string searchTerm, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<CatalogItem>();

            var normalizedSearch = searchTerm.ToLower().Trim();

            return await _context.CatalogItems
                .Include(x => x.CatalogType)
                .Include(x => x.CatalogBrand)
                .Where(x => x.Name.ToLower().Contains(normalizedSearch) ||
                            x.Description.ToLower().Contains(normalizedSearch) ||
                            x.CatalogBrand.Brand.ToLower().Contains(normalizedSearch) ||
                            x.CatalogType.Type.ToLower().Contains(normalizedSearch))
                .OrderByDescending(x => x.Name.ToLower().StartsWith(normalizedSearch)) // Priorité aux correspondances en début
                .ThenBy(x => x.Name)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Recherche les produits avec pagination
        /// </summary>
        public async Task<PaginatedItems<CatalogItem>> SearchPagedAsync(string searchTerm, int pageIndex, int pageSize)
        {
            var query = _context.CatalogItems
                .Include(x => x.CatalogType)
                .Include(x => x.CatalogBrand)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedSearch = searchTerm.ToLower().Trim();
                query = query.Where(x => x.Name.ToLower().Contains(normalizedSearch) ||
                                         x.Description.ToLower().Contains(normalizedSearch) ||
                                         x.CatalogBrand.Brand.ToLower().Contains(normalizedSearch) ||
                                         x.CatalogType.Type.ToLower().Contains(normalizedSearch));
            }

            var totalItems = await query.LongCountAsync();

            var items = await query
                .OrderBy(x => x.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedItems<CatalogItem>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Count = totalItems,
                Data = items
            };
        }

        /// <summary>
        /// Récupère les produits similaires basés sur la catégorie et la marque
        /// </summary>
        public async Task<IEnumerable<CatalogItem>> GetRelatedProductsAsync(Guid productId, int limit = 8)
        {
            var product = await _context.CatalogItems
                .FirstOrDefaultAsync(x => x.Id == productId);

            if (product == null)
                return Enumerable.Empty<CatalogItem>();

            // Récupère les produits de la même catégorie ou marque, excluant le produit actuel
            return await _context.CatalogItems
                .Include(x => x.CatalogType)
                .Include(x => x.CatalogBrand)
                .Where(x => x.Id != productId &&
                           (x.CatalogTypeId == product.CatalogTypeId || x.CatalogBrandId == product.CatalogBrandId))
                .OrderByDescending(x => x.CatalogTypeId == product.CatalogTypeId) // Priorité à la même catégorie
                .ThenBy(x => Guid.NewGuid()) // Randomize pour varier les résultats
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les produits les mieux notés
        /// </summary>
        public async Task<IEnumerable<CatalogItem>> GetTopRatedProductsAsync(int limit = 8)
        {
            // Récupère les produits avec le plus d'avis positifs
            var productIdsWithRatings = await _context.ProductReviews
                .GroupBy(r => r.CatalogItemId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AverageRating = g.Average(r => r.Rating),
                    ReviewCount = g.Count()
                })
                .Where(x => x.ReviewCount >= 1) // Au moins 1 avis
                .OrderByDescending(x => x.AverageRating)
                .ThenByDescending(x => x.ReviewCount)
                .Take(limit)
                .Select(x => x.ProductId)
                .ToListAsync();

            if (!productIdsWithRatings.Any())
            {
                // Si aucun avis, retourne les produits les plus récents
                return await _context.CatalogItems
                    .Include(x => x.CatalogType)
                    .Include(x => x.CatalogBrand)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(limit)
                    .ToListAsync();
            }

            var products = await _context.CatalogItems
                .Include(x => x.CatalogType)
                .Include(x => x.CatalogBrand)
                .Where(x => productIdsWithRatings.Contains(x.Id))
                .ToListAsync();

            // Maintenir l'ordre par rating
            return products.OrderBy(p => productIdsWithRatings.IndexOf(p.Id));
        }

        /// <summary>
        /// Récupère les nouveautés (produits récemment ajoutés)
        /// </summary>
        public async Task<IEnumerable<CatalogItem>> GetNewArrivalsAsync(int limit = 8)
        {
            return await _context.CatalogItems
                .Include(x => x.CatalogType)
                .Include(x => x.CatalogBrand)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les meilleures ventes (basé sur le stock vendu)
        /// Pour l'instant, simule avec les produits ayant le moins de stock (plus vendus)
        /// </summary>
        public async Task<IEnumerable<CatalogItem>> GetBestSellersAsync(int limit = 8)
        {
            // Dans une vraie application, on aurait une table de commandes pour calculer les ventes
            // Ici on simule avec des produits populaires (disponibles avec stock moyen)
            return await _context.CatalogItems
                .Include(x => x.CatalogType)
                .Include(x => x.CatalogBrand)
                .Where(x => x.AvailableStock > 0)
                .OrderBy(x => x.AvailableStock) // Les produits avec moins de stock = plus vendus
                .ThenByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        // ====================================
        // ÉCRITURE (Commands)
        // ====================================

        /// <summary>
        /// Ajoute un nouveau produit
        /// Note : N'appelle PAS SaveChanges, c'est le rôle de UnitOfWork
        /// </summary>
        public async Task<CatalogItem> AddAsync(CatalogItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            await _context.CatalogItems.AddAsync(item);
            
            // Ne PAS appeler SaveChangesAsync ici !
            // C'est le rôle de UnitOfWork
            
            return item;
        }

        /// <summary>
        /// Met à jour un produit existant
        /// Note : N'appelle PAS SaveChanges, c'est le rôle de UnitOfWork
        /// </summary>
        public Task UpdateAsync(CatalogItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            // EF Core suit automatiquement les changements (Change Tracker)
            // Si l'entité est déjà suivie, les modifications sont détectées
            _context.CatalogItems.Update(item);
            
            // Ne PAS appeler SaveChangesAsync ici !
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Supprime un produit (soft delete géré par le DbContext)
        /// Note : N'appelle PAS SaveChanges, c'est le rôle de UnitOfWork
        /// </summary>
        public Task DeleteAsync(Guid id)
        {
            var item = _context.CatalogItems.Find(id);
            
            if (item != null)
            {
                // La suppression sera interceptée par SaveChangesAsync
                // et transformée en soft delete (IsDeleted = true)
                _context.CatalogItems.Remove(item);
            }
            
            // Ne PAS appeler SaveChangesAsync ici !
            
            return Task.CompletedTask;
        }
    }
}


