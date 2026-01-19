using MediatR;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.SearchCatalogItems
{
    /// <summary>
    /// Query pour rechercher des produits avec auto-complétion
    /// </summary>
    public record SearchCatalogItemsQuery(
        string SearchTerm,
        int Limit = 10
    ) : IRequest<IEnumerable<SearchSuggestionDto>>;

    /// <summary>
    /// Query pour rechercher des produits avec pagination complète
    /// </summary>
    public record SearchCatalogItemsPagedQuery(
        string SearchTerm,
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<PaginatedItemsDto<CatalogItemDto>>;
}
