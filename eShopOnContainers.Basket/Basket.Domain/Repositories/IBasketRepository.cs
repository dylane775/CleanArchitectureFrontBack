using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basket.Domain.Entities;

namespace Basket.Domain.Repositories
{

    //C'est ça que joel définit dans la couche application et il appelle ça Interfaces
    public interface IBasketRepository
    {
        Task<CustomerBasket?> GetByIdAsync(Guid id);
        Task<CustomerBasket?> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<CustomerBasket>> GetAllAsync(); // Pour le nettoyage des paniers expirés
        Task<CustomerBasket> AddAsync(CustomerBasket basket);
        Task UpdateAsync(CustomerBasket basket);
        Task DeleteAsync(Guid id);
    }
}