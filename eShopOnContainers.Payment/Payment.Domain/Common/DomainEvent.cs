using System;
using MediatR;

namespace Payment.Domain.Common
{
    public class DomainEvent : INotification
    {
        public DateTime OccurredOn { get; }
        public Guid EventId { get; }

        protected DomainEvent()
        {
            OccurredOn = DateTime.UtcNow;
            EventId = Guid.NewGuid();
        }
    }
}
