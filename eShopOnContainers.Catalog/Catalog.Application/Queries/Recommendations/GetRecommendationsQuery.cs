using System;
using System.Collections.Generic;
using MediatR;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.Recommendations
{
    /// <summary>
    /// Query pour récupérer les produits similaires à un produit donné
    /// </summary>
    public record GetRelatedProductsQuery(Guid ProductId, int Limit = 8) : IRequest<IEnumerable<CatalogItemDto>>;

    /// <summary>
    /// Query pour récupérer les produits les mieux notés
    /// </summary>
    public record GetTopRatedProductsQuery(int Limit = 8) : IRequest<IEnumerable<CatalogItemDto>>;

    /// <summary>
    /// Query pour récupérer les nouveautés
    /// </summary>
    public record GetNewArrivalsQuery(int Limit = 8) : IRequest<IEnumerable<CatalogItemDto>>;

    /// <summary>
    /// Query pour récupérer les meilleures ventes
    /// </summary>
    public record GetBestSellersQuery(int Limit = 8) : IRequest<IEnumerable<CatalogItemDto>>;

    /// <summary>
    /// Query pour récupérer toutes les recommandations de la page d'accueil
    /// </summary>
    public record GetHomeRecommendationsQuery(int Limit = 8) : IRequest<HomeRecommendationsDto>;
}
