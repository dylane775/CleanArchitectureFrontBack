using Identity.Domain.Events;
using Identity.Infrastructure.Messaging.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Messaging.DomainEventHandlers
{
    /// <summary>
    /// Handler that intercepts the UserRegistered Domain Event and publishes an Integration Event to RabbitMQ
    /// </summary>
    public class UserRegisteredDomainEventHandler : INotificationHandler<UserRegisteredDomainEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<UserRegisteredDomainEventHandler> _logger;

        public UserRegisteredDomainEventHandler(
            IPublishEndpoint publishEndpoint,
            ILogger<UserRegisteredDomainEventHandler> logger)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling Domain Event: UserRegistered - UserId: {UserId}, Email: {Email}",
                domainEvent.UserId,
                domainEvent.Email);

            try
            {
                // Transform the Domain Event into an Integration Event
                var integrationEvent = new UserRegisteredIntegrationEvent(
                    userId: domainEvent.UserId,
                    email: domainEvent.Email,
                    firstName: domainEvent.FirstName,
                    lastName: domainEvent.LastName
                );

                // Publish to RabbitMQ via MassTransit
                await _publishEndpoint.Publish(integrationEvent, cancellationToken);

                _logger.LogInformation(
                    "Published Integration Event: UserRegistered - UserId: {UserId}",
                    domainEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing UserRegistered Integration Event - UserId: {UserId}",
                    domainEvent.UserId);

                // Do not propagate the error to avoid blocking the main transaction
            }
        }
    }
}
