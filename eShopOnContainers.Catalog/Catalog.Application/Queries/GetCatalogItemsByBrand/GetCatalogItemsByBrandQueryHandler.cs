using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Application.DTOs.Output;
using MediatR;
using AutoMapper;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Queries.GetCatalogItemsByBrand
{
    public class GetCatalogItemsByBrandQueryHandler : IRequestHandler<GetCatalogItemsByBrandQuery, IEnumerable<CatalogItemDto>>
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly IMapper _mapper;

        public GetCatalogItemsByBrandQueryHandler(ICatalogRepository catalogRepository, IMapper mapper)
        {
            _catalogRepository = catalogRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<CatalogItemDto>> Handle(GetCatalogItemsByBrandQuery request, CancellationToken cancellationToken)
        {
             // 1. Récupérer les produits de la marque
            var catalogItems = await _catalogRepository.GetByBrandAsync(request.BrandId);

            // 2. Mapper vers DTOs et retourner
            return _mapper.Map<IEnumerable<CatalogItemDto>>(catalogItems);
        }
    }
}