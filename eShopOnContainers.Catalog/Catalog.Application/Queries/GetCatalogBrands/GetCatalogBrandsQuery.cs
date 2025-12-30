using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.GetCatalogBrands
{
     public record GetCatalogBrandsQuery : IRequest<IEnumerable<CatalogBrandDto>>
    {
        
    }
}