using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Basket.Application.DTOs.Input;
using Basket.Application.DTOs.Output;
using Basket.Application.Features.Commands.CreateBasket;
using Basket.Application.Features.Commands.AddItemToBasket;
using Basket.Application.Features.Commands.RemoveItemFromBasket;
using Basket.Application.Features.Commands.UpdateItemQuantityCommand;
using Basket.Application.Features.Commands.ClearBasket;
using Basket.Application.Features.Commands.DeleteBasket;
using Basket.Application.Features.Query.GetBasket;
using Basket.Application.Features.Query.GetBasketByCustomer;

namespace Basket.API.Controllers
{
    /// <summary>
    /// Controller pour gérer les paniers clients
    /// Autorise l'accès anonyme pour permettre aux guests (non connectés) d'utiliser le panier
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [AllowAnonymous] // Autoriser l'accès anonyme pour les paniers guests
    public class BasketsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BasketsController> _logger;

        public BasketsController(IMediator mediator, ILogger<BasketsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ====================================
        // 1. CRÉER UN PANIER
        // ====================================

        /// <summary>
        /// Crée un nouveau panier pour un client
        /// </summary>
        /// <param name="dto">Données du panier à créer</param>
        /// <returns>ID du panier créé</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> CreateBasket([FromBody] CreateBasketDto dto)
        {
            _logger.LogInformation("Création du panier pour le client: {CustomerId}", dto.CustomerId);

            var command = new CreateBasketCommand { CustomerId = dto.CustomerId };
            var basketId = await _mediator.Send(command);

            _logger.LogInformation("Panier créé avec succès - ID: {BasketId}", basketId);

            return CreatedAtAction(nameof(GetBasket), new { id = basketId }, basketId);
        }

        // ====================================
        // 2. RÉCUPÉRER UN PANIER PAR ID
        // ====================================

        /// <summary>
        /// Récupère un panier par son ID
        /// </summary>
        /// <param name="id">ID du panier</param>
        /// <returns>Détails du panier</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BasketDto>> GetBasket(Guid id)
        {
            _logger.LogInformation("Récupération du panier {BasketId}", id);

            var query = new GetBasketQuery(id);
            var basket = await _mediator.Send(query);

            if (basket == null)
            {
                _logger.LogWarning("Panier {BasketId} non trouvé", id);
                return NotFound(new { Message = $"Panier {id} non trouvé" });
            }

            return Ok(basket);
        }

        // ====================================
        // 3. RÉCUPÉRER LE PANIER D'UN CLIENT
        // ====================================

        /// <summary>
        /// Récupère le panier d'un client spécifique
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <returns>Détails du panier du client</returns>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BasketDto>> GetBasketByCustomer(string customerId)
        {
            _logger.LogInformation("Récupération du panier du client: {CustomerId}", customerId);

            var query = new GetBasketByCustomerQuery(customerId);
            var basket = await _mediator.Send(query);

            if (basket == null)
            {
                _logger.LogWarning("Aucun panier trouvé pour le client {CustomerId}", customerId);
                return NotFound(new { Message = $"Aucun panier trouvé pour le client {customerId}" });
            }

            return Ok(basket);
        }

        // ====================================
        // 4. AJOUTER UN ITEM AU PANIER
        // ====================================

        /// <summary>
        /// Ajoute un produit au panier
        /// </summary>
        /// <param name="basketId">ID du panier</param>
        /// <param name="dto">Données du produit à ajouter</param>
        /// <returns>Confirmation</returns>
        [HttpPost("{basketId:guid}/items")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddItemToBasket(Guid basketId, [FromBody] AddItemDto dto)
        {
            _logger.LogInformation(
                "Ajout de l'item {CatalogItemId} au panier {BasketId}", 
                dto.CatalogItemId, 
                basketId);

            var command = new AddItemToBasketCommand
            {
                BasketId = basketId,
                CatalogItemId = dto.CatalogItemId,
                ProductName = dto.ProductName,
                UnitPrice = dto.UnitPrice,
                Quantity = dto.Quantity,
                PictureUrl = dto.PictureUrl
            };

            await _mediator.Send(command);

            _logger.LogInformation("Item ajouté avec succès au panier {BasketId}", basketId);

            return NoContent();
        }

        // ====================================
        // 5. SUPPRIMER UN ITEM DU PANIER
        // ====================================

        /// <summary>
        /// Supprime un produit du panier
        /// </summary>
        /// <param name="basketId">ID du panier</param>
        /// <param name="catalogItemId">ID du produit à supprimer</param>
        /// <returns>Confirmation</returns>
        [HttpDelete("{basketId:guid}/items/{catalogItemId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveItemFromBasket(Guid basketId, Guid catalogItemId)
        {
            _logger.LogInformation(
                "Suppression de l'item {CatalogItemId} du panier {BasketId}", 
                catalogItemId, 
                basketId);

            var command = new RemoveItemFromBasketCommand
            {
                BasketId = basketId,
                CatalogItemId = catalogItemId
            };

            await _mediator.Send(command);

            _logger.LogInformation("Item supprimé avec succès du panier {BasketId}", basketId);

            return NoContent();
        }

        // ====================================
        // 6. METTRE À JOUR LA QUANTITÉ D'UN ITEM
        // ====================================

        /// <summary>
        /// Met à jour la quantité d'un produit dans le panier
        /// </summary>
        /// <param name="basketId">ID du panier</param>
        /// <param name="dto">Nouvelles données de quantité</param>
        /// <returns>Confirmation</returns>
        [HttpPut("{basketId:guid}/items")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateItemQuantity(Guid basketId, [FromBody] UpdateQuantityDto dto)
        {
            _logger.LogInformation(
                "Mise à jour de la quantité de l'item {CatalogItemId} dans le panier {BasketId}", 
                dto.CatalogItemId, 
                basketId);

            var command = new UpdateItemQuantityCommand
            {
                BasketId = basketId,
                CatalogItemId = dto.CatalogItemId,
                NewQuantity = dto.NewQuantity
            };

            await _mediator.Send(command);

            _logger.LogInformation("Quantité mise à jour avec succès dans le panier {BasketId}", basketId);

            return NoContent();
        }

        // ====================================
        // 7. VIDER LE PANIER
        // ====================================

        /// <summary>
        /// Vide complètement un panier (supprime tous les items)
        /// </summary>
        /// <param name="basketId">ID du panier à vider</param>
        /// <returns>Confirmation</returns>
        [HttpDelete("{basketId:guid}/clear")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ClearBasket(Guid basketId)
        {
            _logger.LogInformation("Vidage du panier {BasketId}", basketId);

            var command = new ClearBasketCommand { BasketId = basketId };
            await _mediator.Send(command);

            _logger.LogInformation("Panier {BasketId} vidé avec succès", basketId);

            return NoContent();
        }

        // ====================================
        // 8. SUPPRIMER UN PANIER
        // ====================================

        /// <summary>
        /// Supprime un panier (soft delete)
        /// </summary>
        /// <param name="basketId">ID du panier à supprimer</param>
        /// <returns>Confirmation</returns>
        [HttpDelete("{basketId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBasket(Guid basketId)
        {
            _logger.LogInformation("Suppression du panier {BasketId}", basketId);

            var command = new DeleteBasketCommand(basketId);
            await _mediator.Send(command);

            _logger.LogInformation("Panier {BasketId} supprimé avec succès", basketId);

            return NoContent();
        }
    }
}