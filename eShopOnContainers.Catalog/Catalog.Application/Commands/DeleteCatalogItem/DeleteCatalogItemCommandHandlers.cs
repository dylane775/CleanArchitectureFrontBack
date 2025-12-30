using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Catalog.Domain.Repositories;
using Catalog.Domain.Exceptions;
using AutoMapper;
using Catalog.Domain.Entities;

namespace Catalog.Application.Commands.DeleteCatalogItem
{
    public class DeleteCatalogItemCommandHandlers : IRequestHandler<DeleteCatalogItemCommand, Unit>
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly IMapper _mapper;

        public DeleteCatalogItemCommandHandlers(ICatalogBrandRepository catalogBrandRepository, IMapper mapper, ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(DeleteCatalogItemCommand request, CancellationToken cancellationToken)
        {

            var deleteCatalogItem =  await _catalogRepository.GetByIdAsync(request.Id);
            if (deleteCatalogItem is null)
            {
                throw new KeyNotFoundException($"Catalog item with id {request.Id} not found.");
            }

            await  _catalogRepository.UpdateAsync(deleteCatalogItem);
            return Unit.Value;
        }

    }
}