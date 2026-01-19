using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Basket.Domain.Entities;
using System.Reflection;
using Basket.Domain.Common;

namespace Basket.Infrastructure.Data
{
    public class BasketContext : DbContext
    {
        public DbSet<CustomerBasket> CustomerBaskets { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }

        public BasketContext(DbContextOptions<BasketContext> options) : base(options)
        {
        }

        protected BasketContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Exclure DomainEvent du modèle - c'est une classe de base pour les événements
            modelBuilder.Ignore<DomainEvent>();

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        // ✅ SIMPLIFIÉ : Pas d'audit automatique
        // L'audit est géré manuellement dans les handlers
    }
}