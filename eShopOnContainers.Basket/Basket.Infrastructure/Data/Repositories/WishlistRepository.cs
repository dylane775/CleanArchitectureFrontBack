using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Basket.Domain.Entities;
using Basket.Domain.Repositories;

namespace Basket.Infrastructure.Data.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly BasketContext _context;

        public WishlistRepository(BasketContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<WishlistItem>> GetByUserIdAsync(string userId)
        {
            return await _context.WishlistItems
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
        }

        public async Task<WishlistItem> GetByIdAsync(Guid id)
        {
            return await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<WishlistItem> GetByUserAndProductAsync(string userId, Guid catalogItemId)
        {
            return await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.CatalogItemId == catalogItemId);
        }

        public async Task<bool> ExistsAsync(string userId, Guid catalogItemId)
        {
            return await _context.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.CatalogItemId == catalogItemId);
        }

        public async Task<int> GetCountByUserIdAsync(string userId)
        {
            return await _context.WishlistItems
                .CountAsync(w => w.UserId == userId);
        }

        public async Task<WishlistItem> AddAsync(WishlistItem item)
        {
            await _context.WishlistItems.AddAsync(item);
            return item;
        }

        public async Task RemoveAsync(Guid id)
        {
            var item = await _context.WishlistItems.FindAsync(id);
            if (item != null)
            {
                _context.WishlistItems.Remove(item);
            }
        }

        public async Task RemoveByUserAndProductAsync(string userId, Guid catalogItemId)
        {
            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.CatalogItemId == catalogItemId);

            if (item != null)
            {
                _context.WishlistItems.Remove(item);
            }
        }

        public async Task ClearByUserIdAsync(string userId)
        {
            var items = await _context.WishlistItems
                .Where(w => w.UserId == userId)
                .ToListAsync();

            _context.WishlistItems.RemoveRange(items);
        }
    }
}
