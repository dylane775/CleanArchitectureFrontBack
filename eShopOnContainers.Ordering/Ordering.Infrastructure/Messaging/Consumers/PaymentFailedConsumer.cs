using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Commands.CancelOrder;
using Ordering.Infrastructure.Messaging.Events;

namespace Ordering.Infrastructure.Messaging.Consumers
{
    /// <summary>
    /// Consumer qui écoute les événements PaymentFailedEvent depuis le service Payment
    /// et annule automatiquement la commande correspondante
    /// </summary>
    public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentFailedConsumer> _logger;

        public PaymentFailedConsumer(IMediator mediator, ILogger<PaymentFailedConsumer> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Payment failed for Order {OrderId}. Reason: {FailureReason}. Cancelling order...",
                message.OrderId,
                message.FailureReason);

            try
            {
                // Annuler la commande correspondante
                var command = new CancelOrderCommand
                {
                    OrderId = message.OrderId,
                    Reason = $"Paiement échoué: {message.FailureReason}"
                };
                await _mediator.Send(command);

                _logger.LogInformation(
                    "Order {OrderId} has been cancelled due to payment failure",
                    message.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while cancelling Order {OrderId} after payment failure",
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
    /// Événement publié par le service Payment quand un paiement échoue
    /// (Contrat partagé via MassTransit - doit correspondre à Payment.Domain.Events.PaymentFailedEvent)
    /// </summary>
    public record PaymentFailedEvent
    {
        public Guid PaymentId { get; init; }
        public Guid OrderId { get; init; }
        public string CustomerId { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string FailureReason { get; init; } = string.Empty;
        public DateTime FailedAt { get; init; }
    }
}
