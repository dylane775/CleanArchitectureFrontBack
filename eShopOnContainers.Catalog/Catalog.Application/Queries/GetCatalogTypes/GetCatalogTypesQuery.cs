using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.GetCatalogTypes
{
    public record GetCatalogTypesQuery  : IRequest<IEnumerable<CatalogTypeDto>>
    {
     
    }
}