using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Ordering.Domain.Common.Interfaces;

namespace Ordering.Domain.Common
{
    public abstract class Entity : IAuditableEntity
    {
        public Guid Id{ get; protected set; }
        private List<DomainEvent>? _domainEvents;
        public IReadOnlyCollection<DomainEvent>? DomainEvents => _domainEvents?.AsReadOnly();

        // ====== PROPRIÉTÉS D'AUDIT ======
        public DateTime CreatedAt { get; private set; }
        public DateTime? ModifiedAt { get; private set; }
        public string CreatedBy { get; private set; } = string.Empty;
        public string? ModifiedBy { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public string? DeletedBy { get; private set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
        }

        // ====== MÉTHODES D'ÉVÉNEMENTS ======
        public void AddDomainEvent(DomainEvent eventItem)
        {
            _domainEvents ??= new List<DomainEvent>();
            _domainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(DomainEvent eventItem)
        {
            _domainEvents?.Remove(eventItem);
        }
        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }
        // ====== MÉTHODES D'AUDIT ======
        public void SetCreated(string userId, DateTime? when = null)
        {
            CreatedAt = when ?? DateTime.UtcNow;
            CreatedBy = userId ?? throw new ArgumentNullException(nameof(userId));
        }
        public void SetModified(string userId, DateTime? when = null)
        {
            ModifiedAt = when ?? DateTime.UtcNow;
            ModifiedBy = userId;
        }
        public void SetDeleted(string userId, DateTime? when = null)
        {
            IsDeleted = true;
            DeletedAt = when ?? DateTime.UtcNow;
            DeletedBy = userId;
        }
        public void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
        }

         // ====== ÉGALITÉ ======
        public override bool Equals(object? obj)
        {
            if (obj is not Entity other)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (GetType() != other.GetType())
                return false;
            return Id == other.Id;
        }

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(Entity? a, Entity? b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(Entity? a, Entity? b) => !(a == b);
    }
}