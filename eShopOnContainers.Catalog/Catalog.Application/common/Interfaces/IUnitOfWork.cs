using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Application.common.Interfaces
{
    public interface IUnitOfWork
    {
         /// <summary>
        /// Sauvegarde tous les changements en base de données
        /// Publie les Domain Events automatiquement après la sauvegarde
        /// </summary>
        /// <returns>Nombre d'entités modifiées</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Démarre une nouvelle transaction
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Valide (commit) la transaction en cours
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Annule (rollback) la transaction en cours
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}