using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain.Repositories;
using MediatR;
using AutoMapper;
using Catalog.Application.common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Commands.UdapteCatalogItem
{
    public class UpdateCatalogItemCommandHandler : IRequestHandler<UpdateCatalogItemCommand, Unit>  
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateCatalogItemCommandHandler> _logger;

        public UpdateCatalogItemCommandHandler(
            ICatalogRepository catalogRepository, 
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<UpdateCatalogItemCommandHandler> logger)
        {
            _catalogRepository = catalogRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        
        public async Task<Unit> Handle(UpdateCatalogItemCommand request, CancellationToken cancellationToken)
        {
            var catalogItem = await _catalogRepository.GetByIdAsync(request.Id);
            
            if (catalogItem is null)
            {
                throw new KeyNotFoundException($"Catalog item with id {request.Id} not found.");
            }

            _logger.LogWarning("🔥 BEFORE UpdateDetails - OldPrice: {OldPrice}, NewPrice: {NewPrice}", 
                catalogItem.Price, request.Price);
            
            _logger.LogWarning("🔥 BEFORE UpdateDetails - DomainEvents count: {Count}", 
                catalogItem.DomainEvents?.Count ?? 0);

            // Utiliser la méthode métier qui génère le Domain Event
            catalogItem.UpdateDetails(request.Name, request.Description, request.Price);
            catalogItem.SetModified("system");

            _logger.LogWarning("🔥 AFTER UpdateDetails - DomainEvents count: {Count}", 
                catalogItem.DomainEvents?.Count ?? 0);

            await _catalogRepository.UpdateAsync(catalogItem);
            
            _logger.LogWarning("🔥 BEFORE SaveChangesAsync");
            
            // SAUVEGARDER ET PUBLIER LES EVENTS
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogWarning("🔥 AFTER SaveChangesAsync");
            
            return Unit.Value;
        }
    }
}