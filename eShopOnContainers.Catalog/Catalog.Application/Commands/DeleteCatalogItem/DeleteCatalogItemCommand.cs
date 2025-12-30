using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Catalog.Application.Commands.DeleteCatalogItem
{
    public record DeleteCatalogItemCommand : IRequest<Unit>
    {
        public Guid Id ;

        public DeleteCatalogItemCommand(Guid id)
        {
            Id = id;
        }
    }
}