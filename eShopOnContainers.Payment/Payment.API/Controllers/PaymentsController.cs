using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Payment.Application.Commands.InitiatePayment;
using Payment.Application.Commands.ConfirmPayment;
using Payment.Application.Commands.FailPayment;
using Payment.Application.Commands.RefundPayment;
using Payment.Application.Commands.CancelPayment;
using Payment.Application.Queries.GetPaymentById;
using Payment.Application.Queries.GetPaymentByOrderId;
using Payment.Application.Queries.GetPaymentsByCustomerId;
using Payment.Application.Queries.GetPaymentByReference;
using Payment.Application.DTOs.Input;
using Payment.Application.DTOs.Output;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ====================================
        // QUERIES (Lecture)
        // ====================================

        /// <summary>
        /// Récupère un paiement par son ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> GetPaymentById(Guid id)
        {
            _logger.LogInformation("Getting payment {PaymentId}", id);

            try
            {
                var result = await _mediator.Send(new GetPaymentByIdQuery(id));
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment {PaymentId} not found", id);
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Récupère le paiement d'une commande
        /// </summary>
        [HttpGet("order/{orderId:guid}")]
        [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> GetPaymentByOrderId(Guid orderId)
        {
            _logger.LogInformation("Getting payment for order {OrderId}", orderId);

            try
            {
                var result = await _mediator.Send(new GetPaymentByOrderIdQuery(orderId));
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment for order {OrderId} not found", orderId);
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Récupère tous les paiements d'un client
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByCustomerId(string customerId)
        {
            _logger.LogInformation("Getting payments for customer {CustomerId}", customerId);
            var result = await _mediator.Send(new GetPaymentsByCustomerIdQuery(customerId));
            return Ok(result);
        }

        /// <summary>
        /// Récupère un paiement par sa référence
        /// </summary>
        [HttpGet("reference/{reference}")]
        [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> GetPaymentByReference(string reference)
        {
            _logger.LogInformation("Getting payment with reference {Reference}", reference);

            try
            {
                var result = await _mediator.Send(new GetPaymentByReferenceQuery(reference));
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment with reference {Reference} not found", reference);
                return NotFound(new { Message = ex.Message });
            }
        }

        // ====================================
        // COMMANDS (Écriture)
        // ====================================

        /// <summary>
        /// Initie un nouveau paiement
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PaymentInitiatedResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaymentInitiatedResponseDto>> InitiatePayment([FromBody] InitiatePaymentDto dto)
        {
            _logger.LogInformation("Initiating payment for order {OrderId}", dto.OrderId);

            try
            {
                var command = new InitiatePaymentCommand
                {
                    OrderId = dto.OrderId,
                    CustomerId = dto.CustomerId,
                    Amount = dto.Amount,
                    Currency = dto.Currency,
                    PaymentProvider = dto.PaymentProvider,
                    CustomerEmail = dto.CustomerEmail,
                    CustomerPhone = dto.CustomerPhone,
                    Description = dto.Description,
                    CallbackUrl = dto.CallbackUrl,
                    ReturnUrl = dto.ReturnUrl
                };

                var result = await _mediator.Send(command);

                return CreatedAtAction(
                    nameof(GetPaymentById),
                    new { id = result.PaymentId },
                    result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot initiate payment for order {OrderId}", dto.OrderId);
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Confirme un paiement (webhook depuis le provider)
        /// </summary>
        [HttpPost("{id:guid}/confirm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ConfirmPayment(Guid id, [FromBody] ConfirmPaymentDto dto)
        {
            _logger.LogInformation("Confirming payment {PaymentId}", id);

            try
            {
                var command = new ConfirmPaymentCommand(id, dto.TransactionId);
                await _mediator.Send(command);
                return Ok(new { Message = "Payment confirmed successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment {PaymentId} not found", id);
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Marque un paiement comme échoué (webhook depuis le provider)
        /// </summary>
        [HttpPost("{id:guid}/fail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> FailPayment(Guid id, [FromBody] FailPaymentDto dto)
        {
            _logger.LogInformation("Failing payment {PaymentId}", id);

            try
            {
                var command = new FailPaymentCommand(id, dto.FailureReason);
                await _mediator.Send(command);
                return Ok(new { Message = "Payment marked as failed" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment {PaymentId} not found", id);
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Annule un paiement
        /// </summary>
        [HttpPost("{id:guid}/cancel")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CancelPayment(Guid id)
        {
            _logger.LogInformation("Cancelling payment {PaymentId}", id);

            try
            {
                var command = new CancelPaymentCommand(id);
                await _mediator.Send(command);
                return Ok(new { Message = "Payment cancelled successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment {PaymentId} not found", id);
                return NotFound(new { Message = ex.Message });
            }
            catch (Domain.Exceptions.PaymentDomainException ex)
            {
                _logger.LogWarning(ex, "Cannot cancel payment {PaymentId}", id);
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Rembourse un paiement (total ou partiel)
        /// </summary>
        [HttpPost("{id:guid}/refund")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RefundPayment(Guid id, [FromBody] RefundPaymentDto dto)
        {
            _logger.LogInformation("Refunding payment {PaymentId} for amount {Amount}", id, dto.RefundAmount);

            try
            {
                var command = new RefundPaymentCommand(id, dto.RefundAmount, dto.Reason);
                await _mediator.Send(command);
                return Ok(new { Message = "Payment refunded successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment {PaymentId} not found", id);
                return NotFound(new { Message = ex.Message });
            }
            catch (Domain.Exceptions.PaymentDomainException ex)
            {
                _logger.LogWarning(ex, "Cannot refund payment {PaymentId}", id);
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Refund failed for payment {PaymentId}", id);
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Webhook Monetbil (callback après paiement)
        /// </summary>
        [HttpPost("webhook/monetbil")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> MonetbilWebhook([FromBody] MonetbilWebhookDto webhook)
        {
            _logger.LogInformation("Received Monetbil webhook for reference {Reference}", webhook.ItemRef);

            try
            {
                // Récupérer le paiement par référence
                var payment = await _mediator.Send(new GetPaymentByReferenceQuery(webhook.ItemRef));

                // Traiter selon le statut
                if (webhook.Status?.ToLower() == "success" || webhook.Status?.ToLower() == "completed")
                {
                    var confirmCommand = new ConfirmPaymentCommand(payment.Id, webhook.TransactionId);
                    await _mediator.Send(confirmCommand);
                    _logger.LogInformation("Payment {PaymentId} confirmed via webhook", payment.Id);
                }
                else
                {
                    var failCommand = new FailPaymentCommand(payment.Id, webhook.Message ?? "Payment failed");
                    await _mediator.Send(failCommand);
                    _logger.LogInformation("Payment {PaymentId} failed via webhook", payment.Id);
                }

                return Ok(new { Message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Monetbil webhook");
                // Toujours retourner 200 pour éviter que Monetbil ne réessaie indéfiniment
                return Ok(new { Message = "Webhook received but processing failed" });
            }
        }
    }

    // DTO pour le webhook Monetbil
    public class MonetbilWebhookDto
    {
        public string ItemRef { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
