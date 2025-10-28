using AutoMapper;
using FAM.Application.DTOs.Users;
using FAM.Domain.Organizations;
using FAM.Infrastructure.PersistenceModels.Ef;

namespace FAM.Infrastructure.Common.Mapping;

/// <summary>
/// Mapping profile from EF entities directly to DTOs (skip domain to avoid validation)
/// </summary>
public class EfToDtoProfile : Profile
{
    public EfToDtoProfile()
    {
        // UserEf to UserDto (direct mapping - no domain validation)
        CreateMap<UserEf, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
            // Removed UserDevices and UserNodeRoles - use nested resource endpoints instead

        // UserDeviceEf to UserDeviceDto
        CreateMap<UserDeviceEf, UserDeviceDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.DeviceId))
            .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.DeviceName))
            .ForMember(dest => dest.DeviceType, opt => opt.MapFrom(src => src.DeviceType))
            .ForMember(dest => dest.Browser, opt => opt.MapFrom(src => src.Browser))
            .ForMember(dest => dest.OperatingSystem, opt => opt.MapFrom(src => src.OperatingSystem))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt))
            .ForMember(dest => dest.LastActivityAt, opt => opt.MapFrom(src => src.LastActivityAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsTrusted, opt => opt.MapFrom(src => src.IsTrusted));

        // UserNodeRoleEf to UserNodeRoleDto
        CreateMap<UserNodeRoleEf, UserNodeRoleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => src.NodeId))
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : null))
            .ForMember(dest => dest.NodeName, opt => opt.MapFrom(src => src.Node != null ? src.Node.Name : null))
            .ForMember(dest => dest.StartAt, opt => opt.Ignore()) // Not in EF model yet
            .ForMember(dest => dest.EndAt, opt => opt.Ignore())   // Not in EF model yet
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
            .ForMember(dest => dest.Node, opt => opt.MapFrom(src => src.Node));

        // RoleEf to RoleDto
        CreateMap<RoleEf, RoleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Name)); // Using Name as Description

        // OrgNodeEf to OrgNodeDto
        CreateMap<OrgNodeEf, OrgNodeDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ((OrgNodeType)src.Type).ToString()));
    }
}
