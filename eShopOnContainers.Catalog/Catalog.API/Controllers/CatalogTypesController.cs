// ============================================
// FICHIER 1 : CatalogTypesController.cs - COMPLET
// ============================================

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
    /// Controller pour gérer les types/catégories de produits
    /// </summary>
    [ApiController]
    [Route("api/catalog/types")]
    [Produces("application/json")]
    [Authorize]
    public class CatalogTypesController : ControllerBase
    {
        private readonly ICatalogTypeRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CatalogTypesController> _logger;

        public CatalogTypesController(
            ICatalogTypeRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<CatalogTypesController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Récupère tous les types/catégories
        /// </summary>
        /// <returns>Liste de tous les types</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CatalogType>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CatalogType>>> GetAll()
        {
            _logger.LogInformation("Récupération de tous les types");

            var types = await _repository.GetAllAsync();

            return Ok(types);
        }

        /// <summary>
        /// Récupère un type spécifique par son ID
        /// </summary>
        /// <param name="id">ID du type</param>
        /// <returns>Détails du type</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CatalogType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CatalogType>> GetById(Guid id)
        {
            _logger.LogInformation("Récupération du type {TypeId}", id);

            var type = await _repository.GetByIdAsync(id);

            if (type == null)
            {
                _logger.LogWarning("Type {TypeId} non trouvé", id);
                return NotFound(new { Message = $"Type avec l'ID {id} non trouvé" });
            }

            return Ok(type);
        }

        /// <summary>
        /// Crée un nouveau type/catégorie
        /// </summary>
        /// <param name="request">Données du type à créer</param>
        /// <returns>Type créé avec son ID</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CatalogType), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CatalogType>> Create([FromBody] CreateCatalogTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Type))
            {
                return BadRequest(new { Message = "Le nom du type est requis" });
            }

            _logger.LogInformation("Création du type: {TypeName}", request.Type);

            try
            {
                // Créer l'entité
                var catalogType = new CatalogType(request.Type);
                
                // Définir les informations d'audit
                catalogType.SetCreated("system"); // TODO: Remplacer par l'utilisateur actuel

                // Ajouter en base
                var result = await _repository.AddAsync(catalogType);
                
                // Sauvegarder
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Type créé avec succès - ID: {TypeId}", result.Id);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erreur de validation lors de la création du type");
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour un type existant
        /// </summary>
        /// <param name="id">ID du type à modifier</param>
        /// <param name="request">Nouvelles données du type</param>
        /// <returns>Confirmation de la mise à jour</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCatalogTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Type))
            {
                return BadRequest(new { Message = "Le nom du type est requis" });
            }

            _logger.LogInformation("Mise à jour du type {TypeId}", id);

            var catalogType = await _repository.GetByIdAsync(id);

            if (catalogType == null)
            {
                _logger.LogWarning("Type {TypeId} non trouvé", id);
                return NotFound(new { Message = $"Type avec l'ID {id} non trouvé" });
            }

            try
            {
                // Mettre à jour
                catalogType.UpdateType(request.Type);
                catalogType.SetModified("system"); // TODO: Remplacer par l'utilisateur actuel

                // Sauvegarder
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Type {TypeId} mis à jour avec succès", id);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erreur de validation lors de la mise à jour du type");
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Supprime un type (soft delete)
        /// </summary>
        /// <param name="id">ID du type à supprimer</param>
        /// <returns>Confirmation de la suppression</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Suppression du type {TypeId}", id);

            var catalogType = await _repository.GetByIdAsync(id);

            if (catalogType == null)
            {
                _logger.LogWarning("Type {TypeId} non trouvé", id);
                return NotFound(new { Message = $"Type avec l'ID {id} non trouvé" });
            }

            // Soft delete
            catalogType.SetDeleted("system"); // TODO: Remplacer par l'utilisateur actuel

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Type {TypeId} supprimé avec succès", id);

            return NoContent();
        }
    }

    /// <summary>
    /// DTO pour la création d'un type
    /// </summary>
    public record CreateCatalogTypeRequest
    {
        public string Type { get; init; } = string.Empty;
    }

    /// <summary>
    /// DTO pour la mise à jour d'un type
    /// </summary>
    public record UpdateCatalogTypeRequest
    {
        public string Type { get; init; } = string.Empty;
    }
}