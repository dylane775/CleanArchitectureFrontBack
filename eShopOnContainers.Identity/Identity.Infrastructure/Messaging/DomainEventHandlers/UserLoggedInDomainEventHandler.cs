using Identity.Domain.Events;
using Identity.Infrastructure.Messaging.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Messaging.DomainEventHandlers
{
    /// <summary>
    /// Handler that intercepts the UserLoggedIn Domain Event and publishes an Integration Event to RabbitMQ
    /// </summary>
    public class UserLoggedInDomainEventHandler : INotificationHandler<UserLoggedInDomainEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<UserLoggedInDomainEventHandler> _logger;

        public UserLoggedInDomainEventHandler(
            IPublishEndpoint publishEndpoint,
            ILogger<UserLoggedInDomainEventHandler> logger)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(UserLoggedInDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling Domain Event: UserLoggedIn - UserId: {UserId}, Email: {Email}, IpAddress: {IpAddress}",
                domainEvent.UserId,
                domainEvent.Email,
                domainEvent.IpAddress);

            try
            {
                // Transform the Domain Event into an Integration Event
                var integrationEvent = new UserLoggedInIntegrationEvent(
                    userId: domainEvent.UserId,
                    email: domainEvent.Email,
                    ipAddress: domainEvent.IpAddress,
                    loginTime: domainEvent.LoginTime
                );

                // Publish to RabbitMQ via MassTransit
                await _publishEndpoint.Publish(integrationEvent, cancellationToken);

                _logger.LogInformation(
                    "Published Integration Event: UserLoggedIn - UserId: {UserId}",
                    domainEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing UserLoggedIn Integration Event - UserId: {UserId}",
                    domainEvent.UserId);

                // Do not propagate the error to avoid blocking the main transaction
            }
        }
    }
}
