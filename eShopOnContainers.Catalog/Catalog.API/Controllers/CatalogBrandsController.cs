
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Catalog.Application.DTOs.Output;
using Catalog.Domain.Repositories;
using Catalog.Domain.Entities;
using Catalog.Application.common.Interfaces;

namespace Catalog.API.Controllers
{
    /// <summary>
    /// Controller pour gérer les marques de produits
    /// </summary>
    [ApiController]
    [Route("api/catalog/brands")]
    [Produces("application/json")]
    [Authorize]
    public class CatalogBrandsController : ControllerBase
    {
        private readonly ICatalogBrandRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CatalogBrandsController> _logger;

        public CatalogBrandsController(
            ICatalogBrandRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<CatalogBrandsController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Récupère toutes les marques
        /// </summary>
        /// <returns>Liste de toutes les marques</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CatalogBrand>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CatalogBrand>>> GetAll()
        {
            _logger.LogInformation("Récupération de toutes les marques");

            var brands = await _repository.GetAllAsync();

            return Ok(brands);
        }

        /// <summary>
        /// Récupère une marque spécifique par son ID
        /// </summary>
        /// <param name="id">ID de la marque</param>
        /// <returns>Détails de la marque</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CatalogBrand), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CatalogBrand>> GetById(Guid id)
        {
            _logger.LogInformation("Récupération de la marque {BrandId}", id);

            var brand = await _repository.GetByIdAsync(id);

            if (brand == null)
            {
                _logger.LogWarning("Marque {BrandId} non trouvée", id);
                return NotFound(new { Message = $"Marque avec l'ID {id} non trouvée" });
            }

            return Ok(brand);
        }

        /// <summary>
        /// Crée une nouvelle marque
        /// </summary>
        /// <param name="request">Données de la marque à créer</param>
        /// <returns>Marque créée avec son ID</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CatalogBrand), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CatalogBrand>> Create([FromBody] CreateCatalogBrandRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Brand))
            {
                return BadRequest(new { Message = "Le nom de la marque est requis" });
            }

            _logger.LogInformation("Création de la marque: {BrandName}", request.Brand);

            try
            {
                // Créer l'entité
                var catalogBrand = new CatalogBrand(request.Brand);
                
                // Définir les informations d'audit
                catalogBrand.SetCreated("system"); // TODO: Remplacer par l'utilisateur actuel

                // Ajouter en base
                var result = await _repository.AddAsync(catalogBrand);
                
                // Sauvegarder
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Marque créée avec succès - ID: {BrandId}", result.Id);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erreur de validation lors de la création de la marque");
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour une marque existante
        /// </summary>
        /// <param name="id">ID de la marque à modifier</param>
        /// <param name="request">Nouvelles données de la marque</param>
        /// <returns>Confirmation de la mise à jour</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCatalogBrandRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Brand))
            {
                return BadRequest(new { Message = "Le nom de la marque est requis" });
            }

            _logger.LogInformation("Mise à jour de la marque {BrandId}", id);

            var catalogBrand = await _repository.GetByIdAsync(id);

            if (catalogBrand == null)
            {
                _logger.LogWarning("Marque {BrandId} non trouvée", id);
                return NotFound(new { Message = $"Marque avec l'ID {id} non trouvée" });
            }

            try
            {
                // Mettre à jour
                catalogBrand.UpdateBrand(request.Brand);
                catalogBrand.SetModified("system"); // TODO: Remplacer par l'utilisateur actuel

                // Sauvegarder
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Marque {BrandId} mise à jour avec succès", id);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erreur de validation lors de la mise à jour de la marque");
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Supprime une marque (soft delete)
        /// </summary>
        /// <param name="id">ID de la marque à supprimer</param>
        /// <returns>Confirmation de la suppression</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Suppression de la marque {BrandId}", id);

            var catalogBrand = await _repository.GetByIdAsync(id);

            if (catalogBrand == null)
            {
                _logger.LogWarning("Marque {BrandId} non trouvée", id);
                return NotFound(new { Message = $"Marque avec l'ID {id} non trouvée" });
            }

            // Soft delete
            catalogBrand.SetDeleted("system"); // TODO: Remplacer par l'utilisateur actuel

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Marque {BrandId} supprimée avec succès", id);

            return NoContent();
        }
    }

    /// <summary>
    /// DTO pour la création d'une marque
    /// </summary>
    public record CreateCatalogBrandRequest
    {
        public string Brand { get; init; } = string.Empty;
    }

    /// <summary>
    /// DTO pour la mise à jour d'une marque
    /// </summary>
    public record UpdateCatalogBrandRequest
    {
        public string Brand { get; init; } = string.Empty;
    }
}