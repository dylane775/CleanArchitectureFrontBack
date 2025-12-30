using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Entities;
using Ordering.Domain.Repositories;

namespace Ordering.Infrastructure.Data.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly OrderContext _context;

        public OrderItemRepository(OrderContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<OrderItem?> GetByIdAsync(Guid id)
        {
            return await _context.OrderItems
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.OrderItems
                .Where(i => i.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetByCatalogItemIdAsync(Guid catalogItemId)
        {
            return await _context.OrderItems
                .Where(i => i.CatalogItemId == catalogItemId)
                .ToListAsync();
        }

        public async Task<OrderItem> AddAsync(OrderItem orderItem)
        {
            await _context.OrderItems.AddAsync(orderItem);
            return orderItem;
        }

        public Task UpdateAsync(OrderItem orderItem)
        {
            _context.OrderItems.Update(orderItem);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var orderItem = await GetByIdAsync(id);
            if (orderItem != null)
            {
                _context.OrderItems.Remove(orderItem);
            }
        }
    }
}
