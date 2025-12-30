using System;
using Identity.Domain.Common;

namespace Identity.Domain.Events
{
    /// <summary>
    /// Domain event raised when a user confirms their email address
    /// </summary>
    public class EmailConfirmedDomainEvent : BaseDomainEvent
    {
        public Guid UserId { get; }
        public string Email { get; }

        public EmailConfirmedDomainEvent(Guid userId, string email)
        {
            UserId = userId;
            Email = email;
        }
    }
}
