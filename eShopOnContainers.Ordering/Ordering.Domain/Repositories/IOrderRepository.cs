using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ordering.Domain.Entities;

namespace Ordering.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id);//C'est bon
        Task<Order?> GetByIdWithItemsAsync(Guid id);//Pas bon
        Task<IEnumerable<Order>> GetAllAsync();//Pas bon
        Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);//Bon
        Task<IEnumerable<Order>> GetByStatusAsync(string status);//Bon
        Task<IEnumerable<Order>> GetByCustomerIdAndStatusAsync(Guid customerId, string status);//Bon
        Task<Order> AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(Guid id);
    }
}
