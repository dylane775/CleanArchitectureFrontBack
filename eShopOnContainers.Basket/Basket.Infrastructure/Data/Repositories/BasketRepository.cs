using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Basket.Domain.Entities;
using Basket.Domain.Repositories;

namespace Basket.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Implémentation du repository pour CustomerBasket
    /// Gère toutes les opérations de données pour les paniers
    /// </summary>
    public class BasketRepository : IBasketRepository
    {
        private readonly BasketContext _context;

        public BasketRepository(BasketContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ====================================
        // LECTURE (Queries)
        // ====================================

        /// <summary>
        /// Récupère un panier par son ID avec ses items
        /// </summary>
        public async Task<CustomerBasket?> GetByIdAsync(Guid id)
        {
            return await _context.CustomerBaskets
                .Include(b => b.Items)  // Charger les items du panier
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        /// <summary>
        /// Récupère le panier d'un client spécifique
        /// Un client ne devrait avoir qu'un seul panier actif
        /// </summary>
        public async Task<CustomerBasket?> GetByCustomerIdAsync(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("CustomerId cannot be null or empty", nameof(customerId));

            return await _context.CustomerBaskets
                .Include(b => b.Items)  // Charger les items du panier
                .FirstOrDefaultAsync(b => b.CustomerId == customerId);
        }

        // ====================================
        // ÉCRITURE (Commands)
        // ====================================

        /// <summary>
        /// Ajoute un nouveau panier
        /// Note : N'appelle PAS SaveChanges, c'est le rôle de UnitOfWork
        /// </summary>
        public async Task<CustomerBasket> AddAsync(CustomerBasket basket)
        {
            if (basket == null)
                throw new ArgumentNullException(nameof(basket));

            await _context.CustomerBaskets.AddAsync(basket);

            // Ne PAS appeler SaveChangesAsync ici !
            // C'est le rôle de UnitOfWork via les handlers

            return basket;
        }

        /// <summary>
        /// Met à jour un panier existant
        /// Note : N'appelle PAS SaveChanges, c'est le rôle de UnitOfWork
        /// </summary>
        public Task UpdateAsync(CustomerBasket basket)
        {
            if (basket == null)
                throw new ArgumentNullException(nameof(basket));

            // Marquer le panier comme modifié
            _context.CustomerBaskets.Update(basket);

            // ✅ IMPORTANT : Ajouter explicitement les nouveaux items au DbContext
            // EF Core ne détecte pas les changements aux collections ICollection
            // On doit récupérer les items existants en base et comparer
            var existingItems = _context.BasketItems
                .Where(i => i.CustomerBasketId == basket.Id)
                .ToList();

            foreach (var item in basket.Items)
            {
                // Si l'item n'existe pas en base, c'est un nouvel item
                if (!existingItems.Any(ei => ei.Id == item.Id))
                {
                    _context.BasketItems.Add(item);
                }
                else
                {
                    // Sinon, l'item existe déjà, on le marque comme modifié
                    _context.BasketItems.Update(item);
                }
            }

            // Supprimer les items qui ne sont plus dans la collection
            var itemsToRemove = existingItems.Where(ei => !basket.Items.Any(i => i.Id == ei.Id)).ToList();
            foreach (var itemToRemove in itemsToRemove)
            {
                _context.BasketItems.Remove(itemToRemove);
            }

            // Ne PAS appeler SaveChangesAsync ici !

            return Task.CompletedTask;
        }

        /// <summary>
        /// Supprime un panier (soft delete géré par le DbContext)
        /// Note : N'appelle PAS SaveChanges, c'est le rôle de UnitOfWork
        /// </summary>
        public Task DeleteAsync(Guid id)
        {
            var basket = _context.CustomerBaskets.Find(id);

            if (basket != null)
            {
                // La suppression sera interceptée par SaveChangesAsync
                // et transformée en soft delete (IsDeleted = true)
                _context.CustomerBaskets.Remove(basket);
            }

            // Ne PAS appeler SaveChangesAsync ici !

            return Task.CompletedTask;
        }
    }
}