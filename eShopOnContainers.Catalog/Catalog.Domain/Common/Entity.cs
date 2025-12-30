using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain.Common.Interfaces;

namespace Catalog.Domain.Common
{
    /// <summary>
    /// Classe de base pour toutes les entités du domaine
    /// Une entité a une identité unique (Id) et peut générer des événements de domaine
    /// </summary>
    public abstract class Entity : IAuditableEntity
    {
        public Guid Id { get; protected set; }
        private List<DomainEvent> _domainEvents;
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents?.AsReadOnly();

        // ====== PROPRIÉTÉS D'AUDIT ======
        public DateTime CreatedAt { get; private set; }
        public string CreatedBy { get; private set; }
        
        public DateTime? ModifiedAt { get; private set; }
        public string ModifiedBy { get; private set; }
        
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public string DeletedBy { get; private set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
        }

        // ====== MÉTHODES D'ÉVÉNEMENTS DE DOMAINE ======
        
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
        
        public override bool Equals(object obj)
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

        public static bool operator ==(Entity a, Entity b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(Entity a, Entity b) => !(a == b);
    }
}

/*
📚 POURQUOI CES CLASSES DE BASE ?

1. **Entity** :
   - Représente un objet avec une identité unique (Id)
   - Exemple : Un CatalogItem avec Id=123 est différent d'un CatalogItem avec Id=456
   - Peut générer des événements de domaine pour notifier les changements
   - L'égalité est basée sur l'Id, pas sur les propriétés

2. **ValueObject** :
   - Représente un objet défini par ses valeurs, sans identité
   - Exemple : Money(100, "EUR") == Money(100, "EUR") → true
   - Immutable (ne doit pas changer après création)
   - L'égalité est basée sur toutes les propriétés

3. **DomainEvent** :
   - Représente quelque chose qui s'est passé dans le domaine
   - Exemple : "ProductCreatedEvent", "StockUpdatedEvent"
   - Permet la communication asynchrone entre agrégats
   - Aide à maintenir la cohérence éventuelle (eventual consistency)

4. **IAggregateRoot** :
   - Marque les entités qui sont des points d'entrée d'agrégats
   - Garantit l'intégrité transactionnelle à l'intérieur de l'agrégat
   - Exemple : CatalogItem est un agrégat racine*/