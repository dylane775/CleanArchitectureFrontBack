using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Domain.Repositories
{
    public interface IPaymentRepository
    {
        // ====== OPÉRATIONS DE BASE ======
        Task<Entities.Payment> GetByIdAsync(Guid id);
        Task<Entities.Payment> GetByOrderIdAsync(Guid orderId);
        Task<Entities.Payment> GetByTransactionIdAsync(string transactionId);
        Task<Entities.Payment> GetByPaymentReferenceAsync(string paymentReference);

        // ====== QUERIES ======
        Task<IEnumerable<Entities.Payment>> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<Entities.Payment>> GetByStatusAsync(PaymentStatus status);
        Task<IEnumerable<Entities.Payment>> GetByProviderAsync(PaymentProvider provider);
        Task<IEnumerable<Entities.Payment>> GetPendingPaymentsAsync();
        Task<IEnumerable<Entities.Payment>> GetFailedPaymentsAsync();

        // ====== OPÉRATIONS D'ÉCRITURE ======
        Task<Entities.Payment> AddAsync(Entities.Payment payment);
        Task UpdateAsync(Entities.Payment payment);
        Task DeleteAsync(Guid id);

        // ====== VÉRIFICATIONS ======
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsByOrderIdAsync(Guid orderId);

        // ====== UNIT OF WORK ======
        Task<int> SaveChangesAsync();
    }
}
