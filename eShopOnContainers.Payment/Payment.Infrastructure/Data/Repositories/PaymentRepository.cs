using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Payment.Domain.Repositories;
using Payment.Domain.Enums;

namespace Payment.Infrastructure.Data.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentContext _context;

        public PaymentRepository(PaymentContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ====== OPÉRATIONS DE BASE ======
        public async Task<Domain.Entities.Payment> GetByIdAsync(Guid id)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Domain.Entities.Payment> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<Domain.Entities.Payment> GetByTransactionIdAsync(string transactionId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }

        public async Task<Domain.Entities.Payment> GetByPaymentReferenceAsync(string paymentReference)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentReference == paymentReference);
        }

        // ====== QUERIES ======
        public async Task<IEnumerable<Domain.Entities.Payment>> GetByCustomerIdAsync(string customerId)
        {
            return await _context.Payments
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Payment>> GetByStatusAsync(PaymentStatus status)
        {
            return await _context.Payments
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Payment>> GetByProviderAsync(PaymentProvider provider)
        {
            return await _context.Payments
                .Where(p => p.Provider == provider)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Payment>> GetPendingPaymentsAsync()
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Payment>> GetFailedPaymentsAsync()
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Failed)
                .OrderByDescending(p => p.FailedAt)
                .ToListAsync();
        }

        // ====== OPÉRATIONS D'ÉCRITURE ======
        public async Task<Domain.Entities.Payment> AddAsync(Domain.Entities.Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            return payment;
        }

        public Task UpdateAsync(Domain.Entities.Payment payment)
        {
            _context.Payments.Update(payment);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var payment = await GetByIdAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
            }
        }

        // ====== VÉRIFICATIONS ======
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Payments.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> ExistsByOrderIdAsync(Guid orderId)
        {
            return await _context.Payments.AnyAsync(p => p.OrderId == orderId);
        }

        // ====== UNIT OF WORK ======
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
