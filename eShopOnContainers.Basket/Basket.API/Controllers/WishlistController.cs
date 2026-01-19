using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Basket.Application.Commands.Wishlist;
using Basket.Application.Queries.Wishlist;
using Basket.Application.DTOs;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class WishlistController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<WishlistController> _logger;

        public WishlistController(IMediator mediator, ILogger<WishlistController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? throw new UnauthorizedAccessException("User ID not found in token");
        }

        /// <summary>
        /// Récupère la liste de souhaits de l'utilisateur
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<WishlistItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WishlistItemDto>>> GetWishlist()
        {
            var userId = GetUserId();
            _logger.LogInformation("Getting wishlist for user {UserId}", userId);

            var query = new GetWishlistQuery(userId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Récupère le nombre d'articles dans la liste de souhaits
        /// </summary>
        [HttpGet("count")]
        [Authorize]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetWishlistCount()
        {
            var userId = GetUserId();

            var query = new GetWishlistCountQuery(userId);
            var count = await _mediator.Send(query);

            return Ok(count);
        }

        /// <summary>
        /// Vérifie si un produit est dans la liste de souhaits
        /// </summary>
        [HttpGet("check/{catalogItemId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> CheckInWishlist(Guid catalogItemId)
        {
            var userId = GetUserId();

            var query = new CheckWishlistItemQuery(userId, catalogItemId);
            var exists = await _mediator.Send(query);

            return Ok(exists);
        }

        /// <summary>
        /// Ajoute un produit à la liste de souhaits
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(WishlistItemDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WishlistItemDto>> AddToWishlist([FromBody] AddToWishlistDto dto)
        {
            var userId = GetUserId();
            _logger.LogInformation("Adding product {ProductId} to wishlist for user {UserId}", dto.CatalogItemId, userId);

            var command = new AddToWishlistCommand(
                userId,
                dto.CatalogItemId,
                dto.ProductName,
                dto.Price,
                dto.PictureUrl,
                dto.BrandName,
                dto.CategoryName
            );

            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetWishlist), result);
        }

        /// <summary>
        /// Retire un produit de la liste de souhaits
        /// </summary>
        [HttpDelete("{catalogItemId:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveFromWishlist(Guid catalogItemId)
        {
            var userId = GetUserId();
            _logger.LogInformation("Removing product {ProductId} from wishlist for user {UserId}", catalogItemId, userId);

            var command = new RemoveFromWishlistCommand(userId, catalogItemId);
            var removed = await _mediator.Send(command);

            if (!removed)
            {
                return NotFound(new { Message = "Item not found in wishlist" });
            }

            return NoContent();
        }

        /// <summary>
        /// Bascule un produit dans la liste de souhaits (ajoute ou retire)
        /// </summary>
        [HttpPost("toggle/{catalogItemId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult> ToggleWishlist(Guid catalogItemId, [FromBody] AddToWishlistDto dto)
        {
            var userId = GetUserId();

            // Vérifier si le produit est déjà dans la wishlist
            var checkQuery = new CheckWishlistItemQuery(userId, catalogItemId);
            var exists = await _mediator.Send(checkQuery);

            if (exists)
            {
                // Retirer de la wishlist
                var removeCommand = new RemoveFromWishlistCommand(userId, catalogItemId);
                await _mediator.Send(removeCommand);
                return Ok(new { added = false, message = "Removed from wishlist" });
            }
            else
            {
                // Ajouter à la wishlist
                var addCommand = new AddToWishlistCommand(
                    userId,
                    catalogItemId,
                    dto.ProductName,
                    dto.Price,
                    dto.PictureUrl,
                    dto.BrandName,
                    dto.CategoryName
                );
                var result = await _mediator.Send(addCommand);
                return Ok(new { added = true, message = "Added to wishlist", item = result });
            }
        }

        /// <summary>
        /// Vide complètement la liste de souhaits de l'utilisateur
        /// </summary>
        [HttpDelete("clear")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ClearWishlist()
        {
            var userId = GetUserId();
            _logger.LogInformation("Clearing wishlist for user {UserId}", userId);

            var command = new ClearWishlistCommand(userId);
            await _mediator.Send(command);

            return NoContent();
        }
    }
}
