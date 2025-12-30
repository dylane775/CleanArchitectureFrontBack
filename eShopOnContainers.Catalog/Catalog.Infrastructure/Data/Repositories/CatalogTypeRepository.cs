// ============================================
// Fichier : Catalog.Infrastructure/Data/Repositories/CatalogTypeRepository.cs

namespace Catalog.Infrastructure.Data.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Catalog.Domain.Entities;
    using Catalog.Domain.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Implémentation du repository pour CatalogType
    /// </summary>
    public class CatalogTypeRepository : ICatalogTypeRepository
    {
        private readonly CatalogContext _context;

        public CatalogTypeRepository(CatalogContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<CatalogType?> GetByIdAsync(Guid id)
        {
            return await _context.CatalogType
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<CatalogType>> GetAllAsync()
        {
            return await _context.CatalogType
                .OrderBy(x => x.Type)
                .ToListAsync();
        }

        public async Task<CatalogType> AddAsync(CatalogType type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            await _context.CatalogType.AddAsync(type);
            
            return type;
        }
    }
}