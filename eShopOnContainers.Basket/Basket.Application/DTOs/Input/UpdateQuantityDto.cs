using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.Application.DTOs.Input
{
    public record UpdateQuantityDto
    {
        public Guid CatalogItemId { get; init; }
        public int NewQuantity { get; init; }
    }
}