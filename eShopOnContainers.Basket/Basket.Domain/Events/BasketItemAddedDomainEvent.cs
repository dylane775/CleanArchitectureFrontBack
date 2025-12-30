using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basket.Domain.Common;

namespace Basket.Domain.Events
{
    public class BasketItemAddedDomainEvent : DomainEvent
    {
        public Guid BasketId { get; }
        public Guid CatalogItemId { get; }
        public int Quantity { get; }

        public BasketItemAddedDomainEvent(Guid basketId, Guid catalogItemId, int quantity)
        {
            BasketId = basketId;
            CatalogItemId = catalogItemId;
            Quantity = quantity;
        }
    }
        
    }