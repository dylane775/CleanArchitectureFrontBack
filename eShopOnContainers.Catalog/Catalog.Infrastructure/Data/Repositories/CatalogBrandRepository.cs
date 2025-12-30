
// ============================================
// Fichier : Catalog.Infrastructure/Data/Repositories/CatalogBrandRepository.cs

namespace Catalog.Infrastructure.Data.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Catalog.Domain.Entities;
    using Catalog.Domain.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Implémentation du repository pour CatalogBrand
    /// </summary>
    public class CatalogBrandRepository : ICatalogBrandRepository
    {
        private readonly CatalogContext _context;

        public CatalogBrandRepository(CatalogContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<CatalogBrand?> GetByIdAsync(Guid id)
        {
            return await _context.CatalogBrands
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<CatalogBrand>> GetAllAsync()
        {
            return await _context.CatalogBrands
                .OrderBy(x => x.Brand)
                .ToListAsync();
        }

        public async Task<CatalogBrand> AddAsync(CatalogBrand brand)
        {
            if (brand == null)
                throw new ArgumentNullException(nameof(brand));

            await _context.CatalogBrands.AddAsync(brand);
            
            return brand;
        }
    }


    }