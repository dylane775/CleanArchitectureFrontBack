using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain.Entities;

namespace Catalog.Domain.Repositories
{
    public interface ICatalogTypeRepository
    {
        Task<CatalogType> GetByIdAsync(Guid id);
        Task<IEnumerable<CatalogType>> GetAllAsync();
        Task<CatalogType> AddAsync(CatalogType type);
    }
}