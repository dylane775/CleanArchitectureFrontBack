using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.Domain.Entities;

namespace Catalog.Domain.Repositories
{
    public interface IProductReviewRepository
    {
        Task<ProductReview> GetByIdAsync(Guid id);
        Task<IEnumerable<ProductReview>> GetByProductIdAsync(Guid catalogItemId);
        Task<IEnumerable<ProductReview>> GetByUserIdAsync(string userId);
        Task<ProductReview> GetByProductAndUserAsync(Guid catalogItemId, string userId);
        Task<PaginatedItems<ProductReview>> GetPagedByProductIdAsync(Guid catalogItemId, int pageIndex, int pageSize);
        Task<ProductReviewStats> GetStatsByProductIdAsync(Guid catalogItemId);
        Task<Dictionary<Guid, ProductReviewStats>> GetStatsByProductIdsAsync(IEnumerable<Guid> catalogItemIds);
        Task<ProductReview> AddAsync(ProductReview review);
        Task UpdateAsync(ProductReview review);
        Task DeleteAsync(Guid id);
        Task<bool> HasUserReviewedProductAsync(Guid catalogItemId, string userId);
    }

    /// <summary>
    /// Statistiques agrégées des avis pour un produit
    /// </summary>
    public class ProductReviewStats
    {
        public Guid CatalogItemId { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }

        public double FiveStarPercentage => TotalReviews > 0 ? (double)FiveStarCount / TotalReviews * 100 : 0;
        public double FourStarPercentage => TotalReviews > 0 ? (double)FourStarCount / TotalReviews * 100 : 0;
        public double ThreeStarPercentage => TotalReviews > 0 ? (double)ThreeStarCount / TotalReviews * 100 : 0;
        public double TwoStarPercentage => TotalReviews > 0 ? (double)TwoStarCount / TotalReviews * 100 : 0;
        public double OneStarPercentage => TotalReviews > 0 ? (double)OneStarCount / TotalReviews * 100 : 0;
    }
}
