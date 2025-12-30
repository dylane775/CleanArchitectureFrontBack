using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Ordering.Domain.Enums;

namespace Ordering.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrderStatusController : ControllerBase
    {
        /// <summary>
        /// Récupère tous les statuts de commande disponibles
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), StatusCodes.Status200OK)]
        public ActionResult<IReadOnlyCollection<string>> GetAllStatuses()
        {
            return Ok(OrderStatus.GetAll());
        }

        /// <summary>
        /// Vérifie si un statut est valide
        /// </summary>
        [HttpGet("{status}/validate")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public ActionResult<object> ValidateStatus(string status)
        {
            var isValid = OrderStatus.IsValidStatus(status);
            
            return Ok(new
            {
                Status = status,
                IsValid = isValid,
                ValidStatuses = OrderStatus.GetAll()
            });
        }
    }
}