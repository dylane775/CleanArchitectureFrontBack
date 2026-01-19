using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Catalog.Domain.Repositories;
using Catalog.Application.DTOs;
using Catalog.Domain.Entities;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.GetCatalogItems
{

    public class GetCatalogItemsQueryHandler : IRequestHandler<GetCatalogItemsQuery,PaginatedItemsDto<CatalogItemDto>>
    {
       private readonly ICatalogRepository _catalogRepository;
       private readonly IProductReviewRepository _reviewRepository;
       private readonly IMapper _mapper;

       public GetCatalogItemsQueryHandler(
          ICatalogRepository catalogRepository,
          IProductReviewRepository reviewRepository,
          IMapper mapper)
       {
          _catalogRepository = catalogRepository;
          _reviewRepository = reviewRepository;
          _mapper = mapper;
       }

       public async Task<PaginatedItemsDto<CatalogItemDto>> Handle(GetCatalogItemsQuery request, CancellationToken cancellationToken)
       {
         // 1. Récupérer les données paginées depuis le repository
          var paginatedItems = await _catalogRepository.GetPagedAsync(request.PageIndex, request.PageSize);

          // 2. Mapper les entités Domain vers DTOs
          var itemsDto = _mapper.Map<List<CatalogItemDto>>(paginatedItems.Data);

          // 3. Récupérer les statistiques de reviews pour tous les produits en une seule requête
          var productIds = itemsDto.Select(i => i.Id).ToList();
          var reviewStats = await _reviewRepository.GetStatsByProductIdsAsync(productIds);

          // 4. Enrichir les DTOs avec les statistiques de reviews
          var enrichedItems = itemsDto.Select(item =>
          {
              var stats = reviewStats.TryGetValue(item.Id, out var s) ? s : null;
              return item with
              {
                  AverageRating = stats?.AverageRating ?? 0,
                  ReviewCount = stats?.TotalReviews ?? 0
              };
          }).ToList();

          // 5. Retourner les données paginées sous forme de PaginatedItemsDto<CatalogItemDto>
          return new PaginatedItemsDto<CatalogItemDto>(
             pageIndex: paginatedItems.PageIndex,
             pageSize: paginatedItems.PageSize,
             count: paginatedItems.Count,
             data: enrichedItems
          );
       }
    }
}
