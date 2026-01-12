using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Payment.Domain.Common;

namespace Payment.Infrastructure.Data
{
    public class PaymentContext : DbContext
    {
        public DbSet<Domain.Entities.Payment> Payments { get; set; }

        public PaymentContext(DbContextOptions<PaymentContext> options) : base(options)
        {
        }

        protected PaymentContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignorer DomainEvent - ce n'est pas une entité à persister
            modelBuilder.Ignore<DomainEvent>();

            // Appliquer toutes les configurations d'entités
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
