using System;
using Identity.Domain.Common;

namespace Identity.Domain.Events
{
    /// <summary>
    /// Domain event raised when a user successfully logs in
    /// </summary>
    public class UserLoggedInDomainEvent : BaseDomainEvent
    {
        public Guid UserId { get; }
        public string Email { get; }
        public string IpAddress { get; }
        public DateTime LoginTime { get; }

        public UserLoggedInDomainEvent(Guid userId, string email, string ipAddress, DateTime loginTime)
        {
            UserId = userId;
            Email = email;
            IpAddress = ipAddress;
            LoginTime = loginTime;
        }
    }
}
