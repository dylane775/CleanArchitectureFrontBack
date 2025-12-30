using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Catalog.Application.DTOs;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.GetCatalogItems
{
    public record GetCatalogItemsQuery : IRequest<PaginatedItemsDto<CatalogItemDto>>
    {
        public int PageIndex { get; init; }
        public int PageSize { get; init; }

        public GetCatalogItemsQuery(int pageIndex = 0, int pageSize = 10)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
        }
    }
}