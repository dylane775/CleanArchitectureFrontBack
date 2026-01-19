using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Catalog.Domain.Repositories;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.SearchCatalogItems
{
    /// <summary>
    /// Handler pour la recherche avec suggestions (auto-complétion)
    /// </summary>
    public class SearchCatalogItemsQueryHandler : IRequestHandler<SearchCatalogItemsQuery, IEnumerable<SearchSuggestionDto>>
    {
        private readonly ICatalogRepository _catalogRepository;

        public SearchCatalogItemsQueryHandler(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<IEnumerable<SearchSuggestionDto>> Handle(SearchCatalogItemsQuery request, CancellationToken cancellationToken)
        {
            var items = await _catalogRepository.SearchAsync(request.SearchTerm, request.Limit);

            return items.Select(item => new SearchSuggestionDto
            {
                Id = item.Id,
                Name = item.Name,
                Category = item.CatalogType?.Type ?? "",
                Brand = item.CatalogBrand?.Brand ?? "",
                Price = item.Price,
                PictureUri = !string.IsNullOrEmpty(item.PictureUri)
                    ? $"/images/products/{item.PictureUri}"
                    : "",
                Type = "product"
            });
        }
    }

    /// <summary>
    /// Handler pour la recherche paginée complète
    /// </summary>
    public class SearchCatalogItemsPagedQueryHandler : IRequestHandler<SearchCatalogItemsPagedQuery, PaginatedItemsDto<CatalogItemDto>>
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly IProductReviewRepository _reviewRepository;
        private readonly IMapper _mapper;

        public SearchCatalogItemsPagedQueryHandler(
            ICatalogRepository catalogRepository,
            IProductReviewRepository reviewRepository,
            IMapper mapper)
        {
            _catalogRepository = catalogRepository;
            _reviewRepository = reviewRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedItemsDto<CatalogItemDto>> Handle(SearchCatalogItemsPagedQuery request, CancellationToken cancellationToken)
        {
            var paginatedItems = await _catalogRepository.SearchPagedAsync(
                request.SearchTerm,
                request.PageIndex,
                request.PageSize);

            var itemsDto = _mapper.Map<List<CatalogItemDto>>(paginatedItems.Data);

            // Enrichir avec les statistiques de reviews
            var productIds = itemsDto.Select(i => i.Id).ToList();
            var reviewStats = await _reviewRepository.GetStatsByProductIdsAsync(productIds);

            var enrichedItems = itemsDto.Select(item =>
            {
                var stats = reviewStats.TryGetValue(item.Id, out var s) ? s : null;
                return item with
                {
                    AverageRating = stats?.AverageRating ?? 0,
                    ReviewCount = stats?.TotalReviews ?? 0
                };
            }).ToList();

            return new PaginatedItemsDto<CatalogItemDto>(
                paginatedItems.PageIndex,
                paginatedItems.PageSize,
                paginatedItems.Count,
                enrichedItems
            );
        }
    }
}
