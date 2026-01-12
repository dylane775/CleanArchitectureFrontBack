using System.Threading;
using System.Threading.Tasks;
using Payment.Application.Common.Interfaces;

namespace Payment.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PaymentContext _context;

        public UnitOfWork(PaymentContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
