using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Basket.Domain.Entities;

namespace Basket.Domain.Repositories
{
    /// <summary>
    /// Interface pour le repository de la liste de souhaits
    /// </summary>
    public interface IWishlistRepository
    {
        Task<IEnumerable<WishlistItem>> GetByUserIdAsync(string userId);
        Task<WishlistItem> GetByIdAsync(Guid id);
        Task<WishlistItem> GetByUserAndProductAsync(string userId, Guid catalogItemId);
        Task<bool> ExistsAsync(string userId, Guid catalogItemId);
        Task<int> GetCountByUserIdAsync(string userId);
        Task<WishlistItem> AddAsync(WishlistItem item);
        Task RemoveAsync(Guid id);
        Task RemoveByUserAndProductAsync(string userId, Guid catalogItemId);
        Task ClearByUserIdAsync(string userId);
    }
}
