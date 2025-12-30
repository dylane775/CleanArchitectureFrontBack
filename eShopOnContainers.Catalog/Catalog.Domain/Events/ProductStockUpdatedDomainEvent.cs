using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain.Common;

namespace Catalog.Domain.Events
{
    public class ProductStockUpdatedDomainEvent : DomainEvent
    {
         public Guid ProductId { get; }
        public int OldStock { get; }
        public int NewStock { get; }

        public ProductStockUpdatedDomainEvent(Guid productId, int oldStock, int newStock)
        {
            ProductId = productId;
            OldStock = oldStock;
            NewStock = newStock;
        }
    }
}