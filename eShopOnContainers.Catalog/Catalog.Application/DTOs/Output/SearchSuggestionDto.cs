namespace Catalog.Application.DTOs.Output
{
    /// <summary>
    /// DTO pour les suggestions de recherche (auto-complétion)
    /// </summary>
    public record SearchSuggestionDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = "";
        public string Category { get; init; } = "";
        public string Brand { get; init; } = "";
        public decimal Price { get; init; }
        public string PictureUri { get; init; } = "";
        public string Type { get; init; } = "product"; // product, category, brand
    }
}
