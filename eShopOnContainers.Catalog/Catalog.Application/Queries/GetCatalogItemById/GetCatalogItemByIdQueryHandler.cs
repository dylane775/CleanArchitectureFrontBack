using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Application.DTOs.Output;
using MediatR;
using AutoMapper;
using Catalog.Domain.Repositories;
using Catalog.Domain.Exceptions;

namespace Catalog.Application.Queries.GetCatalogItemById
{
    public class GetCatalogItemByIdQueryHandler : IRequestHandler<GetCatalogItemByIdQuery, CatalogItemDto>
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly IMapper _mapper;

        public GetCatalogItemByIdQueryHandler(ICatalogRepository catalogRepository, IMapper mapper)
        {
            _catalogRepository = catalogRepository;
            _mapper = mapper;
        }
        public async Task<CatalogItemDto> Handle(GetCatalogItemByIdQuery request, CancellationToken cancellationToken)
        {
            var catalogItemDto = await _catalogRepository.GetByIdAsync(request.Id);
            if (catalogItemDto is null)
            {
                throw new CatalogDomainException($"Produit avec l'ID {request.Id} non trouvé");
            }
            return _mapper.Map<CatalogItemDto>(catalogItemDto);
        }
    }
}