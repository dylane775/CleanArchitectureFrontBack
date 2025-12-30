using Identity.Domain.Events;
using Identity.Infrastructure.Messaging.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Messaging.DomainEventHandlers
{
    /// <summary>
    /// Handler that intercepts the EmailConfirmed Domain Event and publishes an Integration Event to RabbitMQ
    /// </summary>
    public class EmailConfirmedDomainEventHandler : INotificationHandler<EmailConfirmedDomainEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<EmailConfirmedDomainEventHandler> _logger;

        public EmailConfirmedDomainEventHandler(
            IPublishEndpoint publishEndpoint,
            ILogger<EmailConfirmedDomainEventHandler> logger)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(EmailConfirmedDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling Domain Event: EmailConfirmed - UserId: {UserId}, Email: {Email}",
                domainEvent.UserId,
                domainEvent.Email);

            try
            {
                // Transform the Domain Event into an Integration Event
                var integrationEvent = new EmailConfirmedIntegrationEvent(
                    userId: domainEvent.UserId,
                    email: domainEvent.Email
                );

                // Publish to RabbitMQ via MassTransit
                await _publishEndpoint.Publish(integrationEvent, cancellationToken);

                _logger.LogInformation(
                    "Published Integration Event: EmailConfirmed - UserId: {UserId}",
                    domainEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing EmailConfirmed Integration Event - UserId: {UserId}",
                    domainEvent.UserId);

                // Do not propagate the error to avoid blocking the main transaction
            }
        }
    }
}
