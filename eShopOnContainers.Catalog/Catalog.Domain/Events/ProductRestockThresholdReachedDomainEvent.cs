using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain.Common;

namespace Catalog.Domain.Events
{
    public class ProductRestockThresholdReachedDomainEvent : DomainEvent
    {
         public Guid ProductId { get; }
        public int CurrentStock { get; }
        public int RestockThreshold { get; }

        public ProductRestockThresholdReachedDomainEvent(Guid productId, int currentStock, int restockThreshold)
        {
            ProductId = productId;
            CurrentStock = currentStock;
            RestockThreshold = restockThreshold;
        }
    }
}