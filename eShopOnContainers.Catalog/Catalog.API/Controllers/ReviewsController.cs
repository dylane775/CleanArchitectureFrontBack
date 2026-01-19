using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Catalog.Application.Commands.Reviews;
using Catalog.Application.Queries.Reviews;
using Catalog.Application.DTOs.Input;
using Catalog.Application.DTOs.Output;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    [Produces("application/json")]
    public class ReviewsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IMediator mediator, ILogger<ReviewsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Récupère les avis d'un produit avec les statistiques
        /// </summary>
        [HttpGet("product/{productId:guid}")]
        [ProducesResponseType(typeof(ProductReviewsWithStatsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProductReviewsWithStatsDto>> GetProductReviews(
            Guid productId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Récupération des avis du produit {ProductId}", productId);

            var query = new GetProductReviewsQuery(productId, pageIndex, pageSize);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Récupère les avis d'un utilisateur
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<ProductReviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductReviewDto>>> GetUserReviews(string userId)
        {
            _logger.LogInformation("Récupération des avis de l'utilisateur {UserId}", userId);

            var query = new GetUserReviewsQuery(userId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Crée un nouvel avis
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ProductReviewDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProductReviewDto>> CreateReview([FromBody] CreateReviewDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst("name")?.Value
                ?? "Anonymous";

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            _logger.LogInformation("Création d'un avis par {UserId} pour le produit {ProductId}",
                userId, dto.CatalogItemId);

            try
            {
                var command = new CreateReviewCommand
                {
                    CatalogItemId = dto.CatalogItemId,
                    UserId = userId,
                    UserDisplayName = userName,
                    Rating = dto.Rating,
                    Title = dto.Title,
                    Comment = dto.Comment,
                    IsVerifiedPurchase = false // TODO: Vérifier si l'utilisateur a acheté le produit
                };

                var result = await _mediator.Send(command);

                return CreatedAtAction(
                    nameof(GetProductReviews),
                    new { productId = result.CatalogItemId },
                    result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erreur lors de la création de l'avis");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Supprime un avis
        /// </summary>
        [HttpDelete("{reviewId:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteReview(Guid reviewId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            _logger.LogInformation("Suppression de l'avis {ReviewId} par {UserId}", reviewId, userId);

            try
            {
                var command = new DeleteReviewCommand(reviewId, userId);
                var result = await _mediator.Send(command);

                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Vote sur l'utilité d'un avis
        /// </summary>
        [HttpPost("{reviewId:guid}/vote")]
        [ProducesResponseType(typeof(ProductReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductReviewDto>> VoteReview(Guid reviewId, [FromBody] VoteReviewDto dto)
        {
            _logger.LogInformation("Vote sur l'avis {ReviewId} - IsHelpful: {IsHelpful}",
                reviewId, dto.IsHelpful);

            try
            {
                var command = new VoteReviewCommand(reviewId, dto.IsHelpful);
                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erreur lors du vote sur l'avis");
                return NotFound(ex.Message);
            }
        }
    }
}
