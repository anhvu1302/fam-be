using AutoMapper;
using FAM.Application.DTOs.Users;
using FAM.Domain.Authorization;
using FAM.Domain.Organizations;
using FAM.Domain.Users;
using FAM.Domain.Users.Entities;

namespace FAM.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for User mappings
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // Domain to DTO mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username.Value))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

        CreateMap<UserDto, User>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => FAM.Domain.ValueObjects.Username.Create(src.Username)))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => FAM.Domain.ValueObjects.Email.Create(src.Email)))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

        // Domain entity mappings
        CreateMap<UserDevice, UserDeviceDto>()
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt))
            .ForMember(dest => dest.LastActivityAt, opt => opt.MapFrom(src => src.LastActivityAt));

        CreateMap<UserNodeRole, UserNodeRoleDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : null))
            .ForMember(dest => dest.NodeName, opt => opt.MapFrom(src => src.Node != null ? src.Node.Name : null))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
            .ForMember(dest => dest.Node, opt => opt.MapFrom(src => src.Node));

        CreateMap<Role, RoleDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Name));

        CreateMap<OrgNode, OrgNodeDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
    }
}