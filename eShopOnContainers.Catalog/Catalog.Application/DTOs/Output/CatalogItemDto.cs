using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Application.DTOs.Output
{
     /// <summary>
    /// DTO pour la lecture d'un produit du catalogue
    /// Utilisé pour retourner les données à l'API/Client
    /// </summary>
    public record CatalogItemDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; }="";
        public string Description { get; init; }="";
        public decimal Price { get; init; }
        public string PictureUri { get; init; }="";
        public int AvailableStock { get; init; }
        public bool OnReorder { get; init; }
        
        // Relations
        public Guid CatalogTypeId { get; init; }
        public string CatalogTypeName { get; init; }="";
        
        public Guid CatalogBrandId { get; init; }
        public string CatalogBrandName { get; init; }="";
        
        // Audit (optionnel - à exposer selon les besoins)
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; }="";
        public DateTime? ModifiedAt { get; init; }
        public string ModifiedBy { get; init; }="";
    }
}