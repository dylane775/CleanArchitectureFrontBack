using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Application.DTOs.Output
{
     /// <summary>
    /// DTO pour les marques de catalogue
    /// </summary>
    public record CatalogBrandDto
    {
        public Guid Id { get; init; }
        public string Brand { get; init; } = "";
    }
}