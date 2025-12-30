using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Application.Common.Interfaces
{
    /// <summary>
    /// Unit of Work pattern interface
    /// Manages database transactions and ensures consistency
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Indicates whether there is an active transaction
        /// </summary>
        bool HasActiveTransaction { get; }

        /// <summary>
        /// Saves all pending changes and publishes domain events
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begins a new database transaction
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
