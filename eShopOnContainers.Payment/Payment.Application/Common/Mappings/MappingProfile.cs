using AutoMapper;
using Payment.Application.DTOs.Output;

namespace Payment.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Payment Entity -> PaymentDto
            CreateMap<Domain.Entities.Payment, PaymentDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider.ToString()));
        }
    }
}
