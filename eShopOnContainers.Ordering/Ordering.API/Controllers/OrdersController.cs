using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ordering.Application.Commands.CreateOrder;
using Ordering.Application.Commands.AddItemToOrder;
using Ordering.Application.Commands.RemoveItemFromOrder;
using Ordering.Application.Commands.UpdateOrderItemQuantity;
using Ordering.Application.Commands.SubmitOrder;
using Ordering.Application.Commands.ShipOrder;
using Ordering.Application.Commands.DeliverOrder;
using Ordering.Application.Commands.CancelOrder;
using Ordering.Application.Queries.GetAllOrders;
using Ordering.Application.Queries.GetOrdersByCustomerId;
using Ordering.Application.Queries.GetOrdersByStatus;
using Ordering.Application.Queries.GetOrderByCustomerIdAndStatus;
using Ordering.Application.Queries.GetByIdWithItems;
using Ordering.Application.DTOs.Input;
using Ordering.Application.DTOs.Output;

namespace Ordering.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // Tous les endpoints requièrent une authentification
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ====================================
        // QUERIES (Lecture)
        // ====================================

        /// <summary>
        /// Récupère toutes les commandes
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetAllOrders()
        {
            _logger.LogInformation("Getting all orders");
            var result = await _mediator.Send(new GetAllOrdersQuery());
            return Ok(result);
        }

        /// <summary>
        /// Récupère une commande par son ID avec ses articles
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderItemDto>> GetOrderById(Guid id)
        {
            _logger.LogInformation("Getting order {OrderId}", id);

            try
            {
                var result = await _mediator.Send(new GetByIdWithItems(id));
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order {OrderId} not found", id);
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les commandes d'un client
        /// </summary>
        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersByCustomerId(Guid customerId)
        {
            _logger.LogInformation("Getting orders for customer {CustomerId}", customerId);
            var result = await _mediator.Send(new GetOrdersByCustomerIdQuery(customerId));
            return Ok(result);
        }

        /// <summary>
        /// Récupère les commandes par statut
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersByStatus(string status)
        {
            _logger.LogInformation("Getting orders with status {Status}", status);
            var result = await _mediator.Send(new GetOrdersByStatusQuery(status));
            return Ok(result);
        }

        /// <summary>
        /// Récupère les commandes d'un client par statut
        /// </summary>
        [HttpGet("customer/{customerId:guid}/status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersByCustomerIdAndStatus(
            Guid customerId, string status)
        {
            _logger.LogInformation("Getting orders for customer {CustomerId} with status {Status}", customerId, status);
            var result = await _mediator.Send(new GetOrdersByCustomerIdAndStatusQuery(customerId, status));
            return Ok(result);
        }

        // ====================================
        // COMMANDS (Écriture)
        // ====================================

        /// <summary>
        /// Crée une nouvelle commande
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> CreateOrder([FromBody] CreateOrderDto dto)
        {
            _logger.LogInformation("Creating order for customer {CustomerId}", dto.CustomerId);

            var command = new CreateOrderCommand
            {
                CustomerId = dto.CustomerId,
                ShippingAddress = dto.ShippingAddress,
                BillingAddress = dto.BillingAddress,
                PaymentMethod = dto.PaymentMethod,
                CustomerEmail = dto.CustomerEmail,
                CustomerPhone = dto.CustomerPhone,
                Items = dto.Items
            };

            var orderId = await _mediator.Send(command);

            _logger.LogInformation("Order {OrderId} created successfully", orderId);

            return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, orderId);
        }

        /// <summary>
        /// Ajoute un article à une commande
        /// </summary>
        [HttpPost("{orderId:guid}/items")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddItemToOrder(Guid orderId, [FromBody] AddOrderItemDto dto)
        {
            _logger.LogInformation("Adding item {CatalogItemId} to order {OrderId}", dto.CatalogItemId, orderId);

            try
            {
                var command = new AddItemToOrderCommand
                {
                    OrderId = orderId,
                    CatalogItemId = dto.CatalogItemId,
                    ProductName = dto.ProductName,
                    UnitPrice = dto.UnitPrice,
                    Quantity = dto.Quantity,
                    PictureUrl = dto.PictureUrl,
                    Discount = dto.Discount
                };

                await _mediator.Send(command);

                _logger.LogInformation("Item added to order {OrderId}", orderId);
                return NoContent();
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Supprime un article d'une commande
        /// </summary>
        [HttpDelete("{orderId:guid}/items/{catalogItemId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveItemFromOrder(Guid orderId, Guid catalogItemId)
        {
            _logger.LogInformation("Removing item {CatalogItemId} from order {OrderId}", catalogItemId, orderId);

            try
            {
                var command = new RemoveItemFromOrderCommand
                {
                    OrderId = orderId,
                    CatalogItemId = catalogItemId
                };

                await _mediator.Send(command);

                _logger.LogInformation("Item removed from order {OrderId}", orderId);
                return NoContent();
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour la quantité d'un article
        /// </summary>
        [HttpPut("{orderId:guid}/items/{catalogItemId:guid}/quantity")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateItemQuantity(
            Guid orderId, Guid catalogItemId, [FromBody] UpdateOrderItemQuantityDto dto)
        {
            _logger.LogInformation("Updating quantity for item {CatalogItemId} in order {OrderId}", catalogItemId, orderId);

            try
            {
                var command = new UpdateOrderItemQuantityCommand
                {
                    OrderId = orderId,
                    CatalogItemId = catalogItemId,
                    NewQuantity = dto.NewQuantity
                };

                await _mediator.Send(command);

                _logger.LogInformation("Quantity updated for order {OrderId}", orderId);
                return NoContent();
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        // ====================================
        // WORKFLOW (Changements de statut)
        // ====================================

        /// <summary>
        /// Soumet une commande pour traitement
        /// </summary>
        [HttpPost("{orderId:guid}/submit")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitOrder(Guid orderId)
        {
            _logger.LogInformation("Submitting order {OrderId}", orderId);

            try
            {
                await _mediator.Send(new SubmitOrderCommand { OrderId = orderId });

                _logger.LogInformation("Order {OrderId} submitted", orderId);
                return NoContent();
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex) when (ex.Message.Contains("Cannot"))
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Marque une commande comme expédiée
        /// </summary>
        [HttpPost("{orderId:guid}/ship")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ShipOrder(Guid orderId)
        {
            _logger.LogInformation("Shipping order {OrderId}", orderId);

            try
            {
                await _mediator.Send(new ShipOrderCommand { OrderId = orderId });

                _logger.LogInformation("Order {OrderId} shipped", orderId);
                return NoContent();
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex) when (ex.Message.Contains("Cannot"))
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Marque une commande comme livrée
        /// </summary>
        [HttpPost("{orderId:guid}/deliver")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeliverOrder(Guid orderId)
        {
            _logger.LogInformation("Delivering order {OrderId}", orderId);

            try
            {
                await _mediator.Send(new DeliverOrderCommand { OrderId = orderId });

                _logger.LogInformation("Order {OrderId} delivered", orderId);
                return NoContent();
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex) when (ex.Message.Contains("Cannot"))
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Annule une commande
        /// </summary>
        [HttpPost("{orderId:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelOrder(Guid orderId, [FromBody] CancelOrderDto? dto = null)
        {
            _logger.LogInformation("Cancelling order {OrderId}", orderId);

            try
            {
                var command = new CancelOrderCommand
                {
                    OrderId = orderId,
                    Reason = dto?.Reason
                };

                await _mediator.Send(command);

                _logger.LogInformation("Order {OrderId} cancelled", orderId);
                return NoContent();
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex) when (ex.Message.Contains("Cannot") || ex.Message.Contains("already"))
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}