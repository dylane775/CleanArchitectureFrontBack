using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MediatR;
using Catalog.Application.common.Interfaces;
using Catalog.Domain.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Catalog.Infrastructure.Data
{
    /// <summary>
    /// Implémentation de IUnitOfWork
    /// Gère les transactions et publie les Domain Events automatiquement
    /// </summary>
     public class UnitOfWork : IUnitOfWork
    {
        private readonly CatalogContext _context;
        private readonly IMediator _mediator;
        private IDbContextTransaction? _currentTransaction;

        public UnitOfWork(CatalogContext context, IMediator mediator)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public bool HasActiveTransaction => _currentTransaction != null;

        // ====================================
        // SAUVEGARDE AVEC PUBLICATION DES EVENTS
        // ====================================

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. Sauvegarder les changements en base de données
            var result = await _context.SaveChangesAsync(cancellationToken);

            // 2. Publier les Domain Events APRÈS la sauvegarde
            // Ces events seront interceptés par les handlers qui publieront vers RabbitMQ
            await PublishDomainEventsAsync(cancellationToken);

            return result;
        }

        // ====================================
        // GESTION DES TRANSACTIONS
        // ====================================

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit");
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
                await PublishDomainEventsAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync(cancellationToken);
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        // ====================================
        // PUBLICATION DES DOMAIN EVENTS
        // ====================================

        private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
        {
            // 1. Récupérer toutes les entités qui ont des domain events
            var domainEntities = _context.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
                .Select(x => x.Entity)
                .ToList();

            // 2. Récupérer tous les events
            var domainEvents = domainEntities
                .SelectMany(x => x.DomainEvents!)
                .ToList();

            // 3. Nettoyer les events des entités
            domainEntities.ForEach(entity => entity.ClearDomainEvents());

            // 4. Publier chaque event via MediatR
            // Les handlers (ProductPriceChangedDomainEventHandler, etc.) 
            // vont transformer et publier vers RabbitMQ
            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context?.Dispose();
        }
    }
}