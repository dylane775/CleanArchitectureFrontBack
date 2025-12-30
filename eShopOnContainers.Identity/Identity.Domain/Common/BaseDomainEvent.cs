using System;
using MediatR;

namespace Identity.Domain.Common
{
    /// <summary>
    /// Classe de base pour tous les événements de domaine
    /// </summary>
    public abstract class BaseDomainEvent : INotification
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        protected BaseDomainEvent()
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
    }
}
