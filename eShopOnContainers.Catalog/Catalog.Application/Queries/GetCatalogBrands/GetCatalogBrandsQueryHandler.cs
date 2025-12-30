using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Catalog.Domain.Repositories;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.GetCatalogBrands
{
    public class GetCatalogBrandsQueryHandler : IRequestHandler<GetCatalogBrandsQuery, IEnumerable<CatalogBrandDto>>
    {
        private readonly ICatalogBrandRepository _catalogBrandRepository;
        private readonly IMapper _mapper;

        public GetCatalogBrandsQueryHandler(
            ICatalogBrandRepository catalogBrandRepository,
            IMapper mapper)
        {
            _catalogBrandRepository = catalogBrandRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<CatalogBrandDto>> Handle(GetCatalogBrandsQuery request, CancellationToken cancellationToken)
        {
            // 1. Récupérer toutes les marques
            var catalogBrands = await _catalogBrandRepository.GetAllAsync();

            // 2. Mapper vers DTOs et retourner
            return _mapper.Map<IEnumerable<CatalogBrandDto>>(catalogBrands);
        }
    }
}