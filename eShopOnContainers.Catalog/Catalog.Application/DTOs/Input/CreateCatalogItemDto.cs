using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Application.DTOs.Input
{
    public record CreateCatalogItemDto
    {
         public string Name { get; init; }="";
        public string Description { get; init; }="";
        public decimal Price { get; init; }
        public string PictureFileName { get; init; }="";
        
        public Guid CatalogTypeId { get; init; }
        public Guid CatalogBrandId { get; init; }
        
        public int AvailableStock { get; init; }
        public int RestockThreshold { get; init; }
        public int MaxStockThreshold { get; init; }
    }
}