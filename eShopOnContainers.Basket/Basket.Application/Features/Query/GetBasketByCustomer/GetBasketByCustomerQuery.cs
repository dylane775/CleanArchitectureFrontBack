using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Basket.Application.DTOs.Output;

namespace Basket.Application.Features.Query.GetBasketByCustomer
{
    public record GetBasketByCustomerQuery : IRequest<BasketDto>
    {
        public string CustomerId { get; init; }

        public GetBasketByCustomerQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}