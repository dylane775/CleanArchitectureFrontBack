using AutoMapper;
using Identity.Application.DTOs.Output;
using Identity.Domain.Entities;

namespace Identity.Application.Common.Mappings
{
    /// <summary>
    /// AutoMapper profile for User-related mappings
    /// </summary>
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // User -> UserDto
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.Select(r => r.Name).ToList()));

            // Role -> RoleDto
            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.GetPermissions()));
        }
    }
}
