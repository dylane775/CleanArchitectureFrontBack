using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Catalog.Domain.Repositories;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.GetCatalogItemsByType
{
    public class GetCatalogItemsByTypeQueryHandler : IRequestHandler<GetCatalogItemsByTypeQuery, IEnumerable<CatalogItemDto>>
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly IMapper _mapper;

        public GetCatalogItemsByTypeQueryHandler(ICatalogRepository catalogRepository, IMapper mapper)
        {
            _catalogRepository = catalogRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<CatalogItemDto>> Handle(GetCatalogItemsByTypeQuery request, CancellationToken cancellationToken)
        {
            // 1. Récupérer les produits du type
            var catalogItems = await _catalogRepository.GetByTypeAsync(request.TypeId);

            // 2. Mapper vers DTOs et retourner
            return _mapper.Map<IEnumerable<CatalogItemDto>>(catalogItems);
        }
    }
}