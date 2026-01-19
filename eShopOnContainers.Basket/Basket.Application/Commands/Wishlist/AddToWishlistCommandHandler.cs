using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Basket.Domain.Entities;
using Basket.Domain.Repositories;
using Basket.Application.DTOs;
using Basket.Application.Interfaces;
using Basket.Application.Common.Interfaces;

namespace Basket.Application.Commands.Wishlist
{
    public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, WishlistItemDto>
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddToWishlistCommandHandler(IWishlistRepository wishlistRepository, IUnitOfWork unitOfWork)
        {
            _wishlistRepository = wishlistRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<WishlistItemDto> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
        {
            // Vérifier si le produit est déjà dans la wishlist
            var existingItem = await _wishlistRepository.GetByUserAndProductAsync(request.UserId, request.CatalogItemId);

            if (existingItem != null)
            {
                // Mettre à jour les infos du produit si elles ont changé
                existingItem.UpdateProductInfo(
                    request.ProductName,
                    request.Price,
                    request.PictureUrl,
                    request.BrandName,
                    request.CategoryName
                );
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new WishlistItemDto
                {
                    Id = existingItem.Id,
                    CatalogItemId = existingItem.CatalogItemId,
                    ProductName = existingItem.ProductName,
                    Price = existingItem.Price,
                    PictureUrl = existingItem.PictureUrl,
                    BrandName = existingItem.BrandName,
                    CategoryName = existingItem.CategoryName,
                    AddedAt = existingItem.AddedAt
                };
            }

            // Créer un nouvel item
            var wishlistItem = new WishlistItem(
                request.UserId,
                request.CatalogItemId,
                request.ProductName,
                request.Price,
                request.PictureUrl,
                request.BrandName,
                request.CategoryName
            );

            await _wishlistRepository.AddAsync(wishlistItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new WishlistItemDto
            {
                Id = wishlistItem.Id,
                CatalogItemId = wishlistItem.CatalogItemId,
                ProductName = wishlistItem.ProductName,
                Price = wishlistItem.Price,
                PictureUrl = wishlistItem.PictureUrl,
                BrandName = wishlistItem.BrandName,
                CategoryName = wishlistItem.CategoryName,
                AddedAt = wishlistItem.AddedAt
            };
        }
    }
}
