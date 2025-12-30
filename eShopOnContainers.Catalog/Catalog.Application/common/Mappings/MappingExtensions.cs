using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Catalog.Application.DTOs.Output;
using Catalog.Domain.Repositories;

namespace Catalog.Application.common.Mappings
{
    /// <summary>
    /// Extensions methods pour simplifier l'utilisation d'AutoMapper
    /// </summary>
    public static class MappingExtensions
    {
        /// <summary>
        /// Mappe une collection paginée vers PaginatedItemsDto
        /// </summary>
        public static PaginatedItemsDto<TDestination> MapToPaginatedDto<TSource, TDestination>(
            this IMapper mapper,
            PaginatedItems<TSource> source)
        {
            var mappedData = mapper.Map<IEnumerable<TDestination>>(source.Data);
            
            return new PaginatedItemsDto<TDestination>(
                pageIndex: source.PageIndex,
                pageSize: source.PageSize,
                count: source.Count,
                data: mappedData
            );
        }

         /// <summary>
        /// Projection IQueryable pour AutoMapper
        /// Permet de projeter directement en base de données (plus performant)
        /// </summary>
        public static IQueryable<TDestination> ProjectTo<TDestination>(
            this IQueryable source,
            IMapper mapper)
        {
            return source.ProjectTo<TDestination>(mapper.ConfigurationProvider);
        }
    }
}