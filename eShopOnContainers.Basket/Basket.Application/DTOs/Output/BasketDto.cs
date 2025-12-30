using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.Application.DTOs.Output
{
    public record BasketDto
    {
        public Guid Id { get; init; }
        public string CustomerId { get; init; }
        public List<BasketItemDto> Items { get; init; } = new();
        public decimal TotalPrice { get; init; }
        public int ItemCount { get; init; }
        
        // Audit
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; }
        public DateTime? ModifiedAt { get; init; }
        public string ModifiedBy { get; init; }
    }
}