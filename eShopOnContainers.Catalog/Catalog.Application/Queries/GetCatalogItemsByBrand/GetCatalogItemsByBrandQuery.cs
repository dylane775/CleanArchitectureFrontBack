using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Catalog.Application.DTOs;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.GetCatalogItemsByBrand
{
    public record GetCatalogItemsByBrandQuery : IRequest<IEnumerable<CatalogItemDto>>
    {
        public Guid BrandId { get; init; }

        public GetCatalogItemsByBrandQuery(Guid brandId)
        {
            BrandId = brandId;
        }
    }
}