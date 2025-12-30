using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain.Entities;

namespace Catalog.Domain.Repositories
{
    public interface ICatalogBrandRepository
    {
        Task<CatalogBrand> GetByIdAsync(Guid id);
        Task<IEnumerable<CatalogBrand>> GetAllAsync();
        Task<CatalogBrand> AddAsync(CatalogBrand brand);
    }
}