using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.GetCatalogItemsByType
{
    public record GetCatalogItemsByTypeQuery : IRequest<IEnumerable<CatalogItemDto>>
    {
        public Guid TypeId { get; init; }

        public GetCatalogItemsByTypeQuery(Guid typeId)
        {
            TypeId = typeId;
        }
    }
}