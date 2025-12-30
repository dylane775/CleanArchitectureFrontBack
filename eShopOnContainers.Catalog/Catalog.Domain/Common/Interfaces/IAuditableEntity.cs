using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Domain.Common.Interfaces
{
    public interface IAuditableEntity
    {
       // Création
        DateTime CreatedAt { get; }
        string CreatedBy { get; }

        // Modification
        DateTime? ModifiedAt { get; }
        string ModifiedBy { get; }

        // Soft Delete (suppression logique)
        bool IsDeleted { get; }
        DateTime? DeletedAt { get; }
        string DeletedBy { get; }

        // Méthodes d'audit
        void SetCreated(string userId, DateTime? when = null);
        void SetModified(string userId, DateTime? when = null);
        void SetDeleted(string userId, DateTime? when = null);
        void Restore();
    }
}