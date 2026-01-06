using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using Catalog.Application.DTOs.Output;
using Catalog.Application.DTOs.Input;
using Catalog.Application.Commands.CreateCatalogItem;  // ✅ AJOUTER
using Catalog.Application.Commands.UdapteCatalogItem; // ✅ AJOUTER
using AutoMapper;


namespace Catalog.Application.common.Mappings
{
    public class MappingProfile : Profile
    {
       public MappingProfile()
        {
            // ====================================
            // CATALOG ITEM MAPPINGS
            // ====================================
            
            // Domain → DTO (lecture)
            CreateMap<CatalogItem, CatalogItemDto>()
                // Mappings automatiques pour les propriétés avec le même nom :
                // - Id, Name, Description, Price, AvailableStock, OnReorder
                // - CatalogTypeId, CatalogBrandId
                // - CreatedAt, CreatedBy, ModifiedAt, ModifiedBy

                // Mapping personnalisé pour PictureUri - construire le chemin complet
                .ForMember(dest => dest.PictureUri,
                    opt => opt.MapFrom(src =>
                        !string.IsNullOrEmpty(src.PictureUri)
                            ? $"/images/products/{src.PictureUri}"
                            : string.Empty))

                // Mappings personnalisés pour les navigation properties
                .ForMember(dest => dest.CatalogTypeName,
                    opt => opt.MapFrom(src => src.CatalogType != null ? src.CatalogType.Type : string.Empty))

                .ForMember(dest => dest.CatalogBrandName,
                    opt => opt.MapFrom(src => src.CatalogBrand != null ? src.CatalogBrand.Brand : string.Empty))

                // Mapping pour les spécifications
                .ForMember(dest => dest.Specifications,
                    opt => opt.MapFrom(src => src.Specifications.GetAllAttributes()));

            // DTO → Domain (rarement utilisé, préférer le constructeur)
            // On ne crée généralement PAS ce mapping car on utilise le constructeur
            
             // ✅ AJOUTER: Command → Domain (création)
            // Note: Le constructeur avec paramètres optionnels ne fonctionne pas dans ConstructUsing
            // On ne peut pas mapper directement, on va créer manuellement dans le Handler

            // ✅ AJOUTER: Command → Domain (mise à jour)
            CreateMap<UpdateCatalogItemCommand, CatalogItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Ne pas écraser l'ID
                .AfterMap((cmd, item) => 
                {
                    // Utiliser la méthode métier de l'entité
                    item.UpdateDetails(cmd.Name, cmd.Description, cmd.Price);
                });
            // qui contient les validations métier
            
            // ====================================
            // CATALOG TYPE MAPPINGS
            // ====================================
            
            // Domain → DTO
            CreateMap<CatalogType, CatalogTypeDto>();
            
            // DTO → Domain (pour les commands de création)
            CreateMap<CatalogTypeDto, CatalogType>()
                .ConstructUsing(dto => new CatalogType(dto.Type));

            // ====================================
            // CATALOG BRAND MAPPINGS
            // ====================================
            
            // Domain → DTO
            CreateMap<CatalogBrand, CatalogBrandDto>();
            
            // DTO → Domain (pour les commands de création)
            CreateMap<CatalogBrandDto, CatalogBrand>()
                .ConstructUsing(dto => new CatalogBrand(dto.Brand));

            // ====================================
            // PAGINATED ITEMS MAPPING
            // ====================================
            
            // Mapping générique pour les résultats paginés
            CreateMap(typeof(PaginatedItems<>), typeof(PaginatedItemsDto<>))
                .ForMember("PageIndex", opt => opt.MapFrom("PageIndex"))
                .ForMember("PageSize", opt => opt.MapFrom("PageSize"))
                .ForMember("Count", opt => opt.MapFrom("Count"))
                .ForMember("Data", opt => opt.MapFrom("Data"));
        }
    }
} 
