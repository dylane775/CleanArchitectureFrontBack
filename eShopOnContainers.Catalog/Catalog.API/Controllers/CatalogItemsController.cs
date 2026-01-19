using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Catalog.Application.Commands.CreateCatalogItem;
using Catalog.Application.Commands.UdapteCatalogItem;
using Catalog.Application.Commands.DeleteCatalogItem;
using Catalog.Application.Commands.UpdateStock;
using Catalog.Application.Queries.GetCatalogItems;
using Catalog.Application.Queries.GetCatalogItemById;
using Catalog.Application.Queries.SearchCatalogItems;
using Catalog.Application.Queries.Recommendations;
using Catalog.Application.DTOs.Input;
using Catalog.Application.DTOs.Output;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/catalog/items")]
    [Produces("application/json")]
    public class CatalogItemsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CatalogItemsController> _logger;

        public CatalogItemsController(IMediator mediator, ILogger<CatalogItemsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Récupère tous les produits du catalogue
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedItemsDto<CatalogItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedItemsDto<CatalogItemDto>>> GetAll(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageIndex < 1 || pageSize <= 0 || pageSize > 1000)
            {
                return BadRequest("Paramètres de pagination invalides");
            }

            _logger.LogInformation("Récupération de tous les produits - Page: {PageIndex}, Size: {PageSize}", 
                pageIndex, pageSize);

            var query = new GetCatalogItemsQuery(pageIndex, pageSize);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Récupère un produit par son ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CatalogItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CatalogItemDto>> GetById(Guid id)
        {
            _logger.LogInformation("Récupération du produit {Id}", id);

            var query = new GetCatalogItemByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { Message = $"Produit {id} non trouvé" });
            }

            return Ok(result);
        }

       /// <summary>
        /// Crée un nouveau produit
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CatalogItemDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CatalogItemDto>> Create([FromBody] CreateCatalogItemCommand command)
        {
            _logger.LogInformation("Création du produit: {Name}", command.Name);

            var catalogItem = await _mediator.Send(command);

            _logger.LogInformation("Produit créé avec succès - ID: {Id}", catalogItem.Id);

            return CreatedAtAction(nameof(GetById), new { id = catalogItem.Id }, catalogItem);
        }

        /// <summary>
        /// Met à jour un produit
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCatalogItemCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("L'ID ne correspond pas");
            }

            _logger.LogInformation("Mise à jour du produit {Id}", id);

            await _mediator.Send(command);

            return NoContent();
        }

        /// <summary>
        /// Met à jour le stock d'un produit (ajout ou retrait)
        /// </summary>
        [HttpPatch("{id:guid}/stock")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStock(
            Guid id, 
            [FromBody] UpdateStockDto stockDto)
        {
            if (id != stockDto.ProductId)
            {
                return BadRequest("L'ID ne correspond pas");
            }

            _logger.LogInformation(
                "Mise à jour du stock - Produit: {Id}, Quantité: {Quantity}", 
                id, 
                stockDto.Quantity);

            var command = new UpdateStockCommand(
                catalogItemId: id,
                quantity: stockDto.Quantity,
                isAddStock: stockDto.Quantity > 0
            );

            await _mediator.Send(command);

            return NoContent();
        }

        /// <summary>
        /// Ajoute du stock à un produit
        /// </summary>
        [HttpPost("{id:guid}/stock/add")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddStock(
            Guid id,
            [FromBody] UpdateStockDto stockDto)
        {
            if (id != stockDto.ProductId)
            {
                return BadRequest("L'ID ne correspond pas");
            }

            _logger.LogInformation("Ajout de stock - Produit: {Id}, Quantité: {Quantity}", 
                id, stockDto.Quantity);

            var command = new UpdateStockCommand(
                catalogItemId: id,
                quantity: Math.Abs(stockDto.Quantity), // Force positif
                isAddStock: true
            );

            await _mediator.Send(command);

            return NoContent();
        }

        /// <summary>
        /// Retire du stock d'un produit
        /// </summary>
        [HttpPost("{id:guid}/stock/remove")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveStock(
            Guid id,
            [FromBody] UpdateStockDto stockDto)
        {
            if (id != stockDto.ProductId)
            {
                return BadRequest("L'ID ne correspond pas");
            }

            _logger.LogInformation("Retrait de stock - Produit: {Id}, Quantité: {Quantity}", 
                id, stockDto.Quantity);

            var command = new UpdateStockCommand(
                catalogItemId: id,
                quantity: Math.Abs(stockDto.Quantity), // Force positif
                isAddStock: false
            );

            await _mediator.Send(command);

            return NoContent();
        }

        /// <summary>
        /// Supprime un produit (soft delete)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Suppression du produit {Id}", id);

            var command = new DeleteCatalogItemCommand(id);
            await _mediator.Send(command);

            return NoContent();
        }

        /// <summary>
        /// Recherche des produits avec suggestions (auto-complétion)
        /// </summary>
        [HttpGet("search/suggestions")]
        [ProducesResponseType(typeof(IEnumerable<SearchSuggestionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SearchSuggestionDto>>> GetSearchSuggestions(
            [FromQuery] string q,
            [FromQuery] int limit = 8)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Ok(Enumerable.Empty<SearchSuggestionDto>());
            }

            _logger.LogInformation("Recherche de suggestions pour: {Query}", q);

            var query = new SearchCatalogItemsQuery(q, limit);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Recherche des produits avec pagination
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(PaginatedItemsDto<CatalogItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedItemsDto<CatalogItemDto>>> Search(
            [FromQuery] string q,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageIndex < 1 || pageSize <= 0 || pageSize > 100)
            {
                return BadRequest("Paramètres de pagination invalides");
            }

            _logger.LogInformation("Recherche de produits: {Query} - Page: {PageIndex}", q, pageIndex);

            var query = new SearchCatalogItemsPagedQuery(q ?? "", pageIndex, pageSize);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        // ====================================
        // RECOMMANDATIONS
        // ====================================

        /// <summary>
        /// Récupère les produits similaires à un produit donné
        /// </summary>
        [HttpGet("{id:guid}/related")]
        [ProducesResponseType(typeof(IEnumerable<CatalogItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CatalogItemDto>>> GetRelatedProducts(
            Guid id,
            [FromQuery] int limit = 8)
        {
            _logger.LogInformation("Récupération des produits similaires pour {ProductId}", id);

            var query = new GetRelatedProductsQuery(id, limit);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Récupère les produits les mieux notés
        /// </summary>
        [HttpGet("recommendations/top-rated")]
        [ProducesResponseType(typeof(IEnumerable<CatalogItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CatalogItemDto>>> GetTopRated(
            [FromQuery] int limit = 8)
        {
            _logger.LogInformation("Récupération des produits les mieux notés");

            var query = new GetTopRatedProductsQuery(limit);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Récupère les nouveautés
        /// </summary>
        [HttpGet("recommendations/new-arrivals")]
        [ProducesResponseType(typeof(IEnumerable<CatalogItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CatalogItemDto>>> GetNewArrivals(
            [FromQuery] int limit = 8)
        {
            _logger.LogInformation("Récupération des nouveautés");

            var query = new GetNewArrivalsQuery(limit);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Récupère les meilleures ventes
        /// </summary>
        [HttpGet("recommendations/best-sellers")]
        [ProducesResponseType(typeof(IEnumerable<CatalogItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CatalogItemDto>>> GetBestSellers(
            [FromQuery] int limit = 8)
        {
            _logger.LogInformation("Récupération des meilleures ventes");

            var query = new GetBestSellersQuery(limit);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Récupère toutes les recommandations pour la page d'accueil
        /// </summary>
        [HttpGet("recommendations/home")]
        [ProducesResponseType(typeof(HomeRecommendationsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<HomeRecommendationsDto>> GetHomeRecommendations(
            [FromQuery] int limit = 8)
        {
            _logger.LogInformation("Récupération des recommandations pour la page d'accueil");

            var query = new GetHomeRecommendationsQuery(limit);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
    }
}