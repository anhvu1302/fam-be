using AutoMapper;
using FAM.Application.DTOs.Users;
using FAM.Infrastructure.PersistenceModels.Mongo;

namespace FAM.Infrastructure.Common.Mapping;

/// <summary>
/// Mapping profile from MongoDB entities directly to DTOs (skip domain to avoid validation)
/// </summary>
public class MongoToDtoProfile : Profile
{
    public MongoToDtoProfile()
    {
        // UserMongo to UserDto (direct mapping - no domain validation)
        CreateMap<UserMongo, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DomainId))  // Use DomainId, not MongoDB _id
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
            // Removed UserDevices and UserNodeRoles - use nested resource endpoints instead

        // UserDeviceMongo to UserDeviceDto
        CreateMap<UserDeviceMongo, UserDeviceDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DomainId))  // Use DomainId
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

        // UserNodeRoleMongo to UserNodeRoleDto
        CreateMap<UserNodeRoleMongo, UserNodeRoleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DomainId))  // Use DomainId
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => src.NodeId))
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
            .ForMember(dest => dest.RoleName, opt => opt.Ignore())  // Will be loaded separately if needed
            .ForMember(dest => dest.NodeName, opt => opt.Ignore())  // Will be loaded separately if needed
            .ForMember(dest => dest.StartAt, opt => opt.Ignore())   // Not in Mongo model yet
            .ForMember(dest => dest.EndAt, opt => opt.Ignore())     // Not in Mongo model yet
            .ForMember(dest => dest.Role, opt => opt.Ignore())      // Loaded separately
            .ForMember(dest => dest.Node, opt => opt.Ignore());     // Loaded separately

        // RoleMongo to RoleDto
        CreateMap<RoleMongo, RoleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DomainId))  // Use DomainId
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Name)); // Use Name as Description since Description doesn't exist

        // OrgNodeMongo to OrgNodeDto
        CreateMap<OrgNodeMongo, OrgNodeDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DomainId))  // Use DomainId
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ((FAM.Domain.Organizations.OrgNodeType)src.Type).ToString()));
    }
}
