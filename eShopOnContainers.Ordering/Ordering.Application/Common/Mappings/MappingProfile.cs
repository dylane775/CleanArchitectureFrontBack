using AutoMapper;
using Ordering.Domain.Entities;
using Ordering.Application.DTOs.Output;

namespace Ordering.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ====================================
            // DOMAIN → DTO (Output)
            // ====================================

            // Order → OrderDto
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.TotalItemCount,
                    opt => opt.MapFrom(src => src.GetTotalItemCount()))
                .ForMember(dest => dest.Subtotal,
                    opt => opt.MapFrom(src => src.GetSubtotal()))
                .ForMember(dest => dest.TotalDiscount,
                    opt => opt.MapFrom(src => src.GetTotalDiscount()));

            // Order → OrderSummaryDto (liste simplifiée)
            CreateMap<Order, OrderSummaryDto>()
                .ForMember(dest => dest.ItemCount,
                    opt => opt.MapFrom(src => src.GetTotalItemCount()));

            // OrderItem → OrderItemDto
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.Subtotal,
                    opt => opt.MapFrom(src => src.GetSubtotal()))
                .ForMember(dest => dest.DiscountAmount,
                    opt => opt.MapFrom(src => src.GetDiscountAmount()))
                .ForMember(dest => dest.TotalPrice,
                    opt => opt.MapFrom(src => src.GetTotalPrice()));

            // ====================================
            // COMMAND → DOMAIN (Input)
            // ====================================

            // CreateOrderCommand → Order
            // Note: Pas utilisé directement car le constructeur Order() nécessite des paramètres spécifiques
            // On instancie manuellement dans le handler
        }
    }
}