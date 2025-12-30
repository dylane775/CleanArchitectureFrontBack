using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.Common.Interfaces;
using Identity.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Identity.Infrastructure.Data
{
    /// <summary>
    /// Implementation of IUnitOfWork
    /// Manages transactions and automatically publishes Domain Events
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IdentityDbContext _context;
        private readonly IMediator _mediator;
        private IDbContextTransaction? _currentTransaction;

        public UnitOfWork(IdentityDbContext context, IMediator mediator)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public bool HasActiveTransaction => _currentTransaction != null;

        // ====================================
        // SAVE WITH DOMAIN EVENTS PUBLICATION
        // ====================================

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. Save changes to database
            var result = await _context.SaveChangesAsync(cancellationToken);

            // 2. Publish Domain Events AFTER saving
            // These events will be intercepted by handlers that publish to RabbitMQ
            await PublishDomainEventsAsync(cancellationToken);

            return result;
        }

        // ====================================
        // TRANSACTION MANAGEMENT
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
        // DOMAIN EVENTS PUBLICATION
        // ====================================

        private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
        {
            // 1. Get all entities that have domain events
            // For Identity, we need to check which entities implement domain events
            var domainEntities = _context.ChangeTracker
                .Entries<BaseEntity>()
                .Where(x => x.Entity.GetType().GetProperty("DomainEvents") != null)
                .Select(x => x.Entity)
                .ToList();

            if (!domainEntities.Any())
            {
                return;
            }

            // 2. Get all events from entities
            var domainEvents = domainEntities
                .SelectMany(entity =>
                {
                    var eventsProperty = entity.GetType().GetProperty("DomainEvents");
                    if (eventsProperty != null)
                    {
                        var events = eventsProperty.GetValue(entity) as System.Collections.IEnumerable;
                        if (events != null)
                        {
                            return events.Cast<BaseDomainEvent>();
                        }
                    }
                    return Enumerable.Empty<BaseDomainEvent>();
                })
                .ToList();

            // 3. Clear events from entities
            foreach (var entity in domainEntities)
            {
                var clearMethod = entity.GetType().GetMethod("ClearDomainEvents");
                clearMethod?.Invoke(entity, null);
            }

            // 4. Publish each event via MediatR
            // Handlers (UserRegisteredDomainEventHandler, etc.)
            // will transform and publish to RabbitMQ
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
