namespace Notification.Domain.Common
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTime? CreatedAt { get; protected set; }
        public string? CreatedBy { get; protected set; }
        public DateTime? ModifiedAt { get; protected set; }
        public string? ModifiedBy { get; protected set; }

        public void SetCreated(string? createdBy = null)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = createdBy;
        }

        public void SetModified(string? modifiedBy = null)
        {
            ModifiedAt = DateTime.UtcNow;
            ModifiedBy = modifiedBy;
        }
    }
}
