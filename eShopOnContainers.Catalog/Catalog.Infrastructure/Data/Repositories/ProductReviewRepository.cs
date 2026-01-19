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
    /// Implémentation du repository pour ProductReview
    /// Gère toutes les opérations de données pour les avis clients
    /// </summary>
    public class ProductReviewRepository : IProductReviewRepository
    {
        private readonly CatalogContext _context;

        public ProductReviewRepository(CatalogContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ====================================
        // LECTURE (Queries)
        // ====================================

        public async Task<ProductReview?> GetByIdAsync(Guid id)
        {
            return await _context.ProductReviews
                .Include(r => r.CatalogItem)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<ProductReview>> GetByProductIdAsync(Guid catalogItemId)
        {
            return await _context.ProductReviews
                .Where(r => r.CatalogItemId == catalogItemId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductReview>> GetByUserIdAsync(string userId)
        {
            return await _context.ProductReviews
                .Include(r => r.CatalogItem)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProductReview?> GetByProductAndUserAsync(Guid catalogItemId, string userId)
        {
            return await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.CatalogItemId == catalogItemId && r.UserId == userId);
        }

        public async Task<PaginatedItems<ProductReview>> GetPagedByProductIdAsync(Guid catalogItemId, int pageIndex, int pageSize)
        {
            var query = _context.ProductReviews
                .Where(r => r.CatalogItemId == catalogItemId);

            var totalItems = await query.LongCountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedItems<ProductReview>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Count = totalItems,
                Data = items
            };
        }

        public async Task<ProductReviewStats> GetStatsByProductIdAsync(Guid catalogItemId)
        {
            var reviews = await _context.ProductReviews
                .Where(r => r.CatalogItemId == catalogItemId)
                .ToListAsync();

            return new ProductReviewStats
            {
                CatalogItemId = catalogItemId,
                TotalReviews = reviews.Count,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
                FiveStarCount = reviews.Count(r => r.Rating == 5),
                FourStarCount = reviews.Count(r => r.Rating == 4),
                ThreeStarCount = reviews.Count(r => r.Rating == 3),
                TwoStarCount = reviews.Count(r => r.Rating == 2),
                OneStarCount = reviews.Count(r => r.Rating == 1)
            };
        }

        public async Task<bool> HasUserReviewedProductAsync(Guid catalogItemId, string userId)
        {
            return await _context.ProductReviews
                .AnyAsync(r => r.CatalogItemId == catalogItemId && r.UserId == userId);
        }

        public async Task<Dictionary<Guid, ProductReviewStats>> GetStatsByProductIdsAsync(IEnumerable<Guid> catalogItemIds)
        {
            var productIds = catalogItemIds.ToList();

            var reviewGroups = await _context.ProductReviews
                .Where(r => productIds.Contains(r.CatalogItemId))
                .GroupBy(r => r.CatalogItemId)
                .Select(g => new
                {
                    CatalogItemId = g.Key,
                    TotalReviews = g.Count(),
                    AverageRating = g.Average(r => r.Rating),
                    FiveStarCount = g.Count(r => r.Rating == 5),
                    FourStarCount = g.Count(r => r.Rating == 4),
                    ThreeStarCount = g.Count(r => r.Rating == 3),
                    TwoStarCount = g.Count(r => r.Rating == 2),
                    OneStarCount = g.Count(r => r.Rating == 1)
                })
                .ToListAsync();

            var result = new Dictionary<Guid, ProductReviewStats>();

            foreach (var productId in productIds)
            {
                var stats = reviewGroups.FirstOrDefault(g => g.CatalogItemId == productId);
                result[productId] = stats != null
                    ? new ProductReviewStats
                    {
                        CatalogItemId = productId,
                        TotalReviews = stats.TotalReviews,
                        AverageRating = stats.AverageRating,
                        FiveStarCount = stats.FiveStarCount,
                        FourStarCount = stats.FourStarCount,
                        ThreeStarCount = stats.ThreeStarCount,
                        TwoStarCount = stats.TwoStarCount,
                        OneStarCount = stats.OneStarCount
                    }
                    : new ProductReviewStats
                    {
                        CatalogItemId = productId,
                        TotalReviews = 0,
                        AverageRating = 0
                    };
            }

            return result;
        }

        // ====================================
        // ÉCRITURE (Commands)
        // ====================================

        public async Task<ProductReview> AddAsync(ProductReview review)
        {
            if (review == null)
                throw new ArgumentNullException(nameof(review));

            await _context.ProductReviews.AddAsync(review);
            return review;
        }

        public Task UpdateAsync(ProductReview review)
        {
            if (review == null)
                throw new ArgumentNullException(nameof(review));

            _context.ProductReviews.Update(review);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            var review = _context.ProductReviews.Find(id);

            if (review != null)
            {
                _context.ProductReviews.Remove(review);
            }

            return Task.CompletedTask;
        }
    }
}
