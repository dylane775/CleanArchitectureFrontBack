using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Commands.CreateCatalogItem
{
    public record CreateCatalogItemCommand : IRequest<CatalogItemDto>
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public decimal Price { get; init; }
        public required string PictureFileName { get; init; }
        public Guid CatalogTypeId { get; init; }
        public Guid CatalogBrandId { get; init; }
        public int AvailableStock { get; init; }
        public int RestockThreshold{ get; init; }
        public int MaxStockThreshold { get; init; }
    }
}