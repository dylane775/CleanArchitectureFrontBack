using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Catalog.Domain.Repositories;
using Catalog.Domain.Entities;
using Catalog.Application.common.Interfaces;
using AutoMapper;
using Catalog.Application.Commands.CreateCatalogItem;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Commands.CreateCatalogItemHandlers
{
    public class CreateCatalogItemCommandHandler : IRequestHandler<CreateCatalogItemCommand, CatalogItemDto>
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateCatalogItemCommandHandler(
            ICatalogRepository catalogRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _catalogRepository = catalogRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CatalogItemDto> Handle(CreateCatalogItemCommand request, CancellationToken cancellationToken)
        {
            var catalogItem = _mapper.Map<CatalogItem>(request);
            catalogItem.SetCreated("system");

            catalogItem = await _catalogRepository.AddAsync(catalogItem);

            // ✅ Sauvegarder les changements en base
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Récupérer le produit avec les relations pour le mapper
            var createdItem = await _catalogRepository.GetByIdAsync(catalogItem.Id);

            if (createdItem == null)
                throw new InvalidOperationException($"Failed to retrieve created item with ID {catalogItem.Id}");

            // Mapper vers le DTO
            return _mapper.Map<CatalogItemDto>(createdItem);
        }
    }
}