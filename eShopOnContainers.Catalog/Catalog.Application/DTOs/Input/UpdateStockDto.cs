using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Application.DTOs.Input
{
    public record UpdateStockDto
    {
        public Guid ProductId { get; init; }
        public int Quantity { get; init; }
    }
}