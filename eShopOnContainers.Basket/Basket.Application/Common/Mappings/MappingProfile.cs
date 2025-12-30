using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Basket.Domain.Entities;
using Basket.Application.DTOs.Output;
using Basket.Application.Features.Commands.CreateBasket;
using Basket.Application.Features.Commands.AddItemToBasket;
using Basket.Application.Features.Commands.UpdateItemQuantityCommand;

namespace Basket.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ====================================
            // DOMAIN → DTO (Output)
            // ====================================
            
            // CustomerBasket → BasketDto
            CreateMap<CustomerBasket, BasketDto>()
                .ForMember(dest => dest.TotalPrice, 
                    opt => opt.MapFrom(src => src.GetTotalPrice()))
                .ForMember(dest => dest.ItemCount, 
                    opt => opt.MapFrom(src => src.Items.Count));

            // BasketItem → BasketItemDto
            CreateMap<BasketItem, BasketItemDto>()
                .ForMember(dest => dest.TotalPrice, 
                    opt => opt.MapFrom(src => src.GetTotalPrice()));

            // ====================================
            // COMMAND → DOMAIN (Input)
            // ====================================
            
            // CreateBasketCommand → CustomerBasket
            CreateMap<CreateBasketCommand, CustomerBasket>()
                .ConstructUsing(cmd => new CustomerBasket(cmd.CustomerId));

            // AddItemToBasketCommand → BasketItem (si nécessaire)
            // Note: Pas utilisé directement car on appelle basket.AddItem() avec les paramètres
            
            // UpdateItemQuantityCommand → Paramètres (pas de mapping direct)
        }
    }
}