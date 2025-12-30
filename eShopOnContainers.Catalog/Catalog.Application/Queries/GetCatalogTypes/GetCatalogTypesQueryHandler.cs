using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Catalog.Domain.Repositories;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.GetCatalogTypes
{
    public class GetCatalogTypesQueryHandler : IRequestHandler<GetCatalogTypesQuery, IEnumerable<CatalogTypeDto>>
    {
        private readonly ICatalogTypeRepository _catalogTypeRepository;
        private readonly IMapper _mapper;

         public GetCatalogTypesQueryHandler(
            ICatalogTypeRepository catalogTypeRepository,
            IMapper mapper)
        {
            _catalogTypeRepository = catalogTypeRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<CatalogTypeDto>> Handle(GetCatalogTypesQuery request, CancellationToken cancellationToken)
        {
            // 1. Récupérer tous les types
            var catalogTypes = await _catalogTypeRepository.GetAllAsync();

            // 2. Mapper vers DTOs et retourner
            return _mapper.Map<IEnumerable<CatalogTypeDto>>(catalogTypes);
        }
    }
}