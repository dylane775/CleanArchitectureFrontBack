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
            var items = await _context.CatalogItems
                .Include(x => x.CatalogType)
                .Include(x => x.CatalogBrand)
                .OrderBy(x => x.Name)  // Tri par nom
                .Skip(pageIndex * pageSize)  // Sauter les pages précédentes
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


