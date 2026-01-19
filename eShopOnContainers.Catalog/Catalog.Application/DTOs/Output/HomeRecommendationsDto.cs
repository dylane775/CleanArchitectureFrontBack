using System.Collections.Generic;

namespace Catalog.Application.DTOs.Output
{
    /// <summary>
    /// DTO contenant toutes les recommandations pour la page d'accueil
    /// </summary>
    public class HomeRecommendationsDto
    {
        public IEnumerable<CatalogItemDto> TopRated { get; set; } = new List<CatalogItemDto>();
        public IEnumerable<CatalogItemDto> NewArrivals { get; set; } = new List<CatalogItemDto>();
        public IEnumerable<CatalogItemDto> BestSellers { get; set; } = new List<CatalogItemDto>();
    }
}
