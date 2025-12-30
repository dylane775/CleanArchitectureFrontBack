using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Catalog.Application.Commands.UpdateStock
{
    public record UpdateStockCommand : IRequest<Unit>
    {
        public Guid CatalogItemId { get; init; }
        public int Quantity { get; init; }
        public bool IsAddStock { get; init; }

        public UpdateStockCommand(Guid catalogItemId, int quantity, bool isAddStock)
        {
            CatalogItemId = catalogItemId;
            Quantity = quantity;
            IsAddStock = isAddStock;
        }
    }
}
