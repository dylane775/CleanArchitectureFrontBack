using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Catalog.Domain.Repositories;
using Catalog.Domain.Exceptions;
using Catalog.Application.common.Interfaces; // ✅ Ajouter

namespace Catalog.Application.Commands.UpdateStock
{
    public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, Unit>
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly IUnitOfWork _unitOfWork; // ✅ Injecter IUnitOfWork

        public UpdateStockCommandHandler(ICatalogRepository catalogRepository,IUnitOfWork unitOfWork)
        {
            _catalogRepository = catalogRepository ?? throw new ArgumentNullException(nameof(catalogRepository));
            _unitOfWork = unitOfWork; // ✅ Ajouter
        }

        public async Task<Unit> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
        {
            var catalogItem = await _catalogRepository.GetByIdAsync(request.CatalogItemId);

            if (catalogItem == null)
                throw new CatalogDomainException($"Catalog item with id {request.CatalogItemId} not found");

            if (request.IsAddStock)
            {
                catalogItem.AddStock(request.Quantity);
            }
            else
            {
                catalogItem.RemoveStock(request.Quantity);
            }

            await _catalogRepository.UpdateAsync(catalogItem);
           await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
