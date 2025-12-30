using System;
using Identity.Domain.Common;

namespace Identity.Domain.Events
{
    /// <summary>
    /// Domain event raised when a new user is registered
    /// </summary>
    public class UserRegisteredDomainEvent : BaseDomainEvent
    {
        public Guid UserId { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }

        public UserRegisteredDomainEvent(Guid userId, string email, string firstName, string lastName)
        {
            UserId = userId;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
