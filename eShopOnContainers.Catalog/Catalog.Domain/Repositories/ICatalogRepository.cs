using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain.Entities;


namespace Catalog.Domain.Repositories
{
    public interface ICatalogRepository
    {
        Task<CatalogItem> GetByIdAsync(Guid id);
        Task<IEnumerable<CatalogItem>> GetAllAsync();
        Task<PaginatedItems <CatalogItem>> GetPagedAsync(int pageIndex, int pageSize);
        Task<IEnumerable<CatalogItem>> GetByBrandAsync(Guid brandId);
        Task<IEnumerable<CatalogItem>> GetByTypeAsync(Guid typeId);
        Task<IEnumerable<CatalogItem>> SearchAsync(string searchTerm, int limit = 10);
        Task<PaginatedItems<CatalogItem>> SearchPagedAsync(string searchTerm, int pageIndex, int pageSize);
        Task<IEnumerable<CatalogItem>> GetRelatedProductsAsync(Guid productId, int limit = 8);
        Task<IEnumerable<CatalogItem>> GetTopRatedProductsAsync(int limit = 8);
        Task<IEnumerable<CatalogItem>> GetNewArrivalsAsync(int limit = 8);
        Task<IEnumerable<CatalogItem>> GetBestSellersAsync(int limit = 8);
        Task<CatalogItem> AddAsync(CatalogItem item);
        Task UpdateAsync(CatalogItem item);
        Task DeleteAsync(Guid id);
    }
}