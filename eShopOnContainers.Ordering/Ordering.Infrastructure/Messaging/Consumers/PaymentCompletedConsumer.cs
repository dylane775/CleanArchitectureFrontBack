using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Commands.ConfirmOrder;
using Ordering.Infrastructure.Messaging.Events;

namespace Ordering.Infrastructure.Messaging.Consumers
{
    /// <summary>
    /// Consumer qui écoute les événements PaymentCompletedEvent depuis le service Payment
    /// et confirme automatiquement la commande correspondante
    /// </summary>
    public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentCompletedConsumer> _logger;

        public PaymentCompletedConsumer(IMediator mediator, ILogger<PaymentCompletedConsumer> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Payment completed for Order {OrderId}. Transaction: {TransactionId}. Confirming order...",
                message.OrderId,
                message.TransactionId);

            try
            {
                // Confirmer la commande correspondante
                var command = new ConfirmOrderCommand
                {
                    OrderId = message.OrderId
                };
                await _mediator.Send(command);

                _logger.LogInformation(
                    "Order {OrderId} has been confirmed after successful payment",
                    message.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while confirming Order {OrderId} after payment completion",
                    message.OrderId);

                // Relancer l'exception pour que MassTransit puisse gérer le retry
                throw;
            }
        }
    }
}

namespace Ordering.Infrastructure.Messaging.Events
{
    /// <summary>
    /// Événement publié par le service Payment quand un paiement réussit
    /// (Contrat partagé via MassTransit - doit correspondre à Payment.Domain.Events.PaymentCompletedEvent)
    /// </summary>
    public record PaymentCompletedEvent
    {
        public Guid PaymentId { get; init; }
        public Guid OrderId { get; init; }
        public string CustomerId { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string? TransactionId { get; init; }
        public DateTime CompletedAt { get; init; }
    }
}
