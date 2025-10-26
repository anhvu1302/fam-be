using AutoMapper;
using FAM.Application.DTOs.Users;
using FAM.Domain.Users;

namespace FAM.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for User mappings
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username.Value))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

        CreateMap<UserDto, User>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => FAM.Domain.ValueObjects.Username.Create(src.Username)))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => FAM.Domain.ValueObjects.Email.Create(src.Email)))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));
    }
}