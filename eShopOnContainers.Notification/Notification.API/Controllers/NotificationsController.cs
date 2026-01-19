using MediatR;
using Microsoft.AspNetCore.Mvc;
using Notification.API.Services;
using Notification.Application.Commands.CreateNotification;
using Notification.Application.Commands.DeleteNotification;
using Notification.Application.Commands.MarkAsRead;
using Notification.Application.DTOs;
using Notification.Application.Queries.GetNotifications;

namespace Notification.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            IMediator mediator,
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _mediator = mediator;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUserNotifications(
            Guid userId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20)
        {
            var notifications = await _mediator.Send(new GetNotificationsQuery(userId, skip, take));
            return Ok(notifications);
        }

        [HttpGet("user/{userId}/unread")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUnreadNotifications(Guid userId)
        {
            var notifications = await _mediator.Send(new GetUnreadNotificationsQuery(userId));
            return Ok(notifications);
        }

        [HttpGet("user/{userId}/count")]
        public async Task<ActionResult<NotificationCountDto>> GetNotificationCount(Guid userId)
        {
            var count = await _mediator.Send(new GetNotificationCountQuery(userId));
            return Ok(count);
        }

        [HttpPost]
        public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            var command = new CreateNotificationCommand(
                dto.UserId,
                dto.Type,
                dto.Title,
                dto.Message,
                dto.ImageUrl,
                dto.ActionUrl,
                dto.RelatedEntityId,
                dto.Metadata
            );

            var notification = await _mediator.Send(command);

            // Envoyer la notification en temps réel via SignalR
            await _notificationService.SendNotificationAsync(notification);

            // Mettre à jour le compteur de notifications non lues
            var count = await _mediator.Send(new GetNotificationCountQuery(dto.UserId));
            await _notificationService.SendUnreadCountUpdateAsync(dto.UserId, count.UnreadCount);

            return CreatedAtAction(nameof(GetUserNotifications), new { userId = dto.UserId }, notification);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result = await _mediator.Send(new MarkNotificationAsReadCommand(id));

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPut("user/{userId}/read-all")]
        public async Task<IActionResult> MarkAllAsRead(Guid userId)
        {
            await _mediator.Send(new MarkAllNotificationsAsReadCommand(userId));

            // Mettre à jour le compteur via SignalR
            await _notificationService.SendUnreadCountUpdateAsync(userId, 0);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var result = await _mediator.Send(new DeleteNotificationCommand(id));

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("user/{userId}/all")]
        public async Task<IActionResult> DeleteAllNotifications(Guid userId)
        {
            await _mediator.Send(new DeleteAllNotificationsCommand(userId));

            // Mettre à jour le compteur via SignalR
            await _notificationService.SendUnreadCountUpdateAsync(userId, 0);

            return NoContent();
        }

        [HttpPost("test/{userId}")]
        public async Task<ActionResult<NotificationDto>> SendTestNotification(Guid userId)
        {
            var command = new CreateNotificationCommand(
                userId,
                "PROMO_ALERT",
                "Offre Spéciale!",
                "Bénéficiez de -20% sur votre prochaine commande avec le code PROMO20",
                null,
                "/catalog",
                null,
                new Dictionary<string, string> { { "promoCode", "PROMO20" } }
            );

            var notification = await _mediator.Send(command);
            await _notificationService.SendNotificationAsync(notification);

            var count = await _mediator.Send(new GetNotificationCountQuery(userId));
            await _notificationService.SendUnreadCountUpdateAsync(userId, count.UnreadCount);

            return Ok(notification);
        }
    }
}
