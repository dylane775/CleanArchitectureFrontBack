using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basket.Domain.Common;

namespace Basket.Domain.Events
{
    public class BasketItemRemovedDomainEvent : DomainEvent
    {
        public Guid BasketId { get; }
        public Guid CatalogItemId { get; }

        public BasketItemRemovedDomainEvent(Guid basketId, Guid catalogItemId)
        {
            BasketId = basketId;
            CatalogItemId = catalogItemId;
        }
    }
        
    }