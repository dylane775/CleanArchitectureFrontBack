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
       private readonly IMapper _mapper; 

       public GetCatalogItemsQueryHandler(ICatalogRepository catalogRepository, IMapper mapper)
       {
          _catalogRepository = catalogRepository;
          _mapper = mapper;
       }

       public async Task<PaginatedItemsDto<CatalogItemDto>> Handle(GetCatalogItemsQuery request, CancellationToken cancellationToken)
       {
         // 1. Récupérer les données paginées depuis le repository
          var paginatedItems = await _catalogRepository.GetPagedAsync(request.PageIndex, request.PageSize);
          // 2. Mapper les entités Domain vers DTOs
          var itemsDto = _mapper.Map<IEnumerable<CatalogItemDto>>(paginatedItems.Data);

            // 3. Retourner les données paginées sous forme de PaginatedItemsDto<CatalogItemDto>
          return new PaginatedItemsDto<CatalogItemDto>(
             pageIndex: paginatedItems.PageIndex,
                pageSize: paginatedItems.PageSize,
                count: paginatedItems.Count,
                data: itemsDto
          );

       }
    }
}