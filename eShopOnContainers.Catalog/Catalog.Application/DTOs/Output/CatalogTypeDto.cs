using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Application.DTOs.Output
{
    /// <summary>
    /// DTO pour les types/catégories de catalogue
    /// </summary>
    public record CatalogTypeDto
    {
        public Guid Id { get; init; }
        public string Type { get; init; }="";
    }
}