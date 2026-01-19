using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Catalog.Domain.Repositories;
using Catalog.Application.DTOs.Output;

namespace Catalog.Application.Queries.Recommendations
{
    public class GetRelatedProductsQueryHandler : IRequestHandler<GetRelatedProductsQuery, IEnumerable<CatalogItemDto>>
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetRelatedProductsQueryHandler(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<IEnumerable<CatalogItemDto>> Handle(GetRelatedProductsQuery request, CancellationToken cancellationToken)
        {
            var items = await _catalogRepository.GetRelatedProductsAsync(request.ProductId, request.Limit);

            return items.Select(item => new CatalogItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                PictureUri = !string.IsNullOrEmpty(item.PictureUri) ? $"/images/products/{item.PictureUri}" : string.Empty,
                AvailableStock = item.AvailableStock,
                CatalogBrandId = item.CatalogBrandId,
                CatalogBrandName = item.CatalogBrand?.Brand ?? string.Empty,
                CatalogTypeId = item.CatalogTypeId,
                CatalogTypeName = item.CatalogType?.Type ?? string.Empty,
                Specifications = item.Specifications?.GetAllAttributes()
            });
        }
    }

    public class GetTopRatedProductsQueryHandler : IRequestHandler<GetTopRatedProductsQuery, IEnumerable<CatalogItemDto>>
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetTopRatedProductsQueryHandler(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<IEnumerable<CatalogItemDto>> Handle(GetTopRatedProductsQuery request, CancellationToken cancellationToken)
        {
            var items = await _catalogRepository.GetTopRatedProductsAsync(request.Limit);

            return items.Select(item => new CatalogItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                PictureUri = !string.IsNullOrEmpty(item.PictureUri) ? $"/images/products/{item.PictureUri}" : string.Empty,
                AvailableStock = item.AvailableStock,
                CatalogBrandId = item.CatalogBrandId,
                CatalogBrandName = item.CatalogBrand?.Brand ?? string.Empty,
                CatalogTypeId = item.CatalogTypeId,
                CatalogTypeName = item.CatalogType?.Type ?? string.Empty,
                Specifications = item.Specifications?.GetAllAttributes()
            });
        }
    }

    public class GetNewArrivalsQueryHandler : IRequestHandler<GetNewArrivalsQuery, IEnumerable<CatalogItemDto>>
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetNewArrivalsQueryHandler(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<IEnumerable<CatalogItemDto>> Handle(GetNewArrivalsQuery request, CancellationToken cancellationToken)
        {
            var items = await _catalogRepository.GetNewArrivalsAsync(request.Limit);

            return items.Select(item => new CatalogItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                PictureUri = !string.IsNullOrEmpty(item.PictureUri) ? $"/images/products/{item.PictureUri}" : string.Empty,
                AvailableStock = item.AvailableStock,
                CatalogBrandId = item.CatalogBrandId,
                CatalogBrandName = item.CatalogBrand?.Brand ?? string.Empty,
                CatalogTypeId = item.CatalogTypeId,
                CatalogTypeName = item.CatalogType?.Type ?? string.Empty,
                Specifications = item.Specifications?.GetAllAttributes()
            });
        }
    }

    public class GetBestSellersQueryHandler : IRequestHandler<GetBestSellersQuery, IEnumerable<CatalogItemDto>>
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetBestSellersQueryHandler(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<IEnumerable<CatalogItemDto>> Handle(GetBestSellersQuery request, CancellationToken cancellationToken)
        {
            var items = await _catalogRepository.GetBestSellersAsync(request.Limit);

            return items.Select(item => new CatalogItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                PictureUri = !string.IsNullOrEmpty(item.PictureUri) ? $"/images/products/{item.PictureUri}" : string.Empty,
                AvailableStock = item.AvailableStock,
                CatalogBrandId = item.CatalogBrandId,
                CatalogBrandName = item.CatalogBrand?.Brand ?? string.Empty,
                CatalogTypeId = item.CatalogTypeId,
                CatalogTypeName = item.CatalogType?.Type ?? string.Empty,
                Specifications = item.Specifications?.GetAllAttributes()
            });
        }
    }

    public class GetHomeRecommendationsQueryHandler : IRequestHandler<GetHomeRecommendationsQuery, HomeRecommendationsDto>
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetHomeRecommendationsQueryHandler(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<HomeRecommendationsDto> Handle(GetHomeRecommendationsQuery request, CancellationToken cancellationToken)
        {
            // Récupère toutes les recommandations en parallèle
            var topRatedTask = _catalogRepository.GetTopRatedProductsAsync(request.Limit);
            var newArrivalsTask = _catalogRepository.GetNewArrivalsAsync(request.Limit);
            var bestSellersTask = _catalogRepository.GetBestSellersAsync(request.Limit);

            await Task.WhenAll(topRatedTask, newArrivalsTask, bestSellersTask);

            return new HomeRecommendationsDto
            {
                TopRated = (await topRatedTask).Select(item => new CatalogItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    PictureUri = !string.IsNullOrEmpty(item.PictureUri) ? $"/images/products/{item.PictureUri}" : string.Empty,
                    AvailableStock = item.AvailableStock,
                    CatalogBrandId = item.CatalogBrandId,
                    CatalogBrandName = item.CatalogBrand?.Brand ?? string.Empty,
                    CatalogTypeId = item.CatalogTypeId,
                    CatalogTypeName = item.CatalogType?.Type ?? string.Empty,
                    Specifications = item.Specifications?.GetAllAttributes()
                }),
                NewArrivals = (await newArrivalsTask).Select(item => new CatalogItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    PictureUri = !string.IsNullOrEmpty(item.PictureUri) ? $"/images/products/{item.PictureUri}" : string.Empty,
                    AvailableStock = item.AvailableStock,
                    CatalogBrandId = item.CatalogBrandId,
                    CatalogBrandName = item.CatalogBrand?.Brand ?? string.Empty,
                    CatalogTypeId = item.CatalogTypeId,
                    CatalogTypeName = item.CatalogType?.Type ?? string.Empty,
                    Specifications = item.Specifications?.GetAllAttributes()
                }),
                BestSellers = (await bestSellersTask).Select(item => new CatalogItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    PictureUri = !string.IsNullOrEmpty(item.PictureUri) ? $"/images/products/{item.PictureUri}" : string.Empty,
                    AvailableStock = item.AvailableStock,
                    CatalogBrandId = item.CatalogBrandId,
                    CatalogBrandName = item.CatalogBrand?.Brand ?? string.Empty,
                    CatalogTypeId = item.CatalogTypeId,
                    CatalogTypeName = item.CatalogType?.Type ?? string.Empty,
                    Specifications = item.Specifications?.GetAllAttributes()
                })
            };
        }
    }
}
