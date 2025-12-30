using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.GetCatalogItemById
{
    public record GetCatalogItemByIdQuery : IRequest<CatalogItemDto>
    {
         public Guid Id { get; init; }

        public GetCatalogItemByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}