using System;

namespace Payment.Domain.Common.Interfaces
{
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; }
        string CreatedBy { get; }
        DateTime? ModifiedAt { get; }
        string ModifiedBy { get; }
        bool IsDeleted { get; }
        DateTime? DeletedAt { get; }
        string DeletedBy { get; }

        void SetCreated(string userId, DateTime? when = null);
        void SetModified(string userId, DateTime? when = null);
        void SetDeleted(string userId, DateTime? when = null);
        void Restore();
    }
}
