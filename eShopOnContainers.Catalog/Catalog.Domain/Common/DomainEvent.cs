using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Catalog.Domain.Common
{
    public class DomainEvent : INotification
    {
        // Date et heure de création de l'événement
        public DateTime OccurredOn { get; }
        public Guid EventId { get; }

        protected DomainEvent()
        {
            OccurredOn = DateTime.Now;
            EventId = Guid.NewGuid();
        }
    }
}