using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.Application.DTOs.Input
{
    public record CreateBasketDto
    {
        public string CustomerId { get; init; }
    }
}