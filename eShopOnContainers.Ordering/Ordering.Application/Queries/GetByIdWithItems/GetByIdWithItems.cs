using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Ordering.Application.DTOs.Output;

namespace Ordering.Application.Queries.GetByIdWithItems
{
    public class GetByIdWithItems : IRequest<OrderDto>
    {
       public Guid Id { get; set; }

        public GetByIdWithItems(Guid id)
        {
            Id = id;
        }
    }
}