using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Application.DTOs.Output
{
    public record PaginatedItemsDto<T>
    {
         public int PageIndex { get; init; }
        public int PageSize { get; init; }
        public long Count { get; init; }
        public IEnumerable<T> Data { get; init; }

        public PaginatedItemsDto(int pageIndex, int pageSize, long count, IEnumerable<T> data)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Count = count;
            Data = data;
        }
    }
}