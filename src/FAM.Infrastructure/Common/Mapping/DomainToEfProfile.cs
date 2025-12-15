using AutoMapper;

using FAM.Domain.Authorization;
using FAM.Domain.Common.Entities;
using FAM.Domain.EmailTemplates;
using FAM.Domain.Organizations;
using FAM.Domain.Users;
using FAM.Domain.Users.Entities;
using FAM.Domain.ValueObjects;
using FAM.Infrastructure.PersistenceModels.Ef;

namespace FAM.Infrastructure.Common.Mapping;

/// <summary>
/// Mapping profile for EF persistence models
/// </summary>
public class DomainToEfProfile : Profile
{
    public DomainToEfProfile()
    {
        // Company mappings removed

        CreateMap<User, UserEf>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username.Value))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password.Hash))
            .ForMember(dest => dest.PasswordSalt, opt => opt.MapFrom(src => src.Password.Salt))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
            .ForMember(dest => dest.PhoneNumber,
                opt => opt.MapFrom(src => src.PhoneNumber != null ? src.PhoneNumber.Value : null))
            .ForMember(dest => dest.PhoneCountryCode,
                opt => opt.MapFrom(src => src.PhoneNumber != null ? src.PhoneNumber.CountryCode : null))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Bio))
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => src.TwoFactorEnabled))
            .ForMember(dest => dest.TwoFactorSecret, opt => opt.MapFrom(src => src.TwoFactorSecret))
            .ForMember(dest => dest.TwoFactorBackupCodes, opt => opt.MapFrom(src => src.TwoFactorBackupCodes))
            .ForMember(dest => dest.TwoFactorSetupDate, opt => opt.MapFrom(src => src.TwoFactorSetupDate))
            .ForMember(dest => dest.PendingTwoFactorSecret, opt => opt.MapFrom(src => src.PendingTwoFactorSecret))
            .ForMember(dest => dest.PendingTwoFactorSecretExpiresAt,
                opt => opt.MapFrom(src => src.PendingTwoFactorSecretExpiresAt))
            .ForMember(dest => dest.PasswordResetToken, opt => opt.MapFrom(src => src.PasswordResetToken))
            .ForMember(dest => dest.PasswordResetTokenExpiresAt,
                opt => opt.MapFrom(src => src.PasswordResetTokenExpiresAt))
            .ForMember(dest => dest.PasswordChangedAt, opt => opt.MapFrom(src => src.PasswordChangedAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => src.IsEmailVerified))
            .ForMember(dest => dest.IsPhoneVerified, opt => opt.MapFrom(src => src.IsPhoneVerified))
            .ForMember(dest => dest.EmailVerifiedAt, opt => opt.MapFrom(src => src.EmailVerifiedAt))
            .ForMember(dest => dest.PhoneVerifiedAt, opt => opt.MapFrom(src => src.PhoneVerifiedAt))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt))
            .ForMember(dest => dest.LastLoginIp, opt => opt.MapFrom(src => src.LastLoginIp))
            .ForMember(dest => dest.FailedLoginAttempts, opt => opt.MapFrom(src => src.FailedLoginAttempts))
            .ForMember(dest => dest.LockoutEnd, opt => opt.MapFrom(src => src.LockoutEnd))
            .ForMember(dest => dest.PreferredLanguage, opt => opt.MapFrom(src => src.PreferredLanguage))
            .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone))
            .ForMember(dest => dest.ReceiveNotifications, opt => opt.MapFrom(src => src.ReceiveNotifications))
            .ForMember(dest => dest.ReceiveMarketingEmails, opt => opt.MapFrom(src => src.ReceiveMarketingEmails))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<UserEf, User>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => Username.Create(src.Username)))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => Email.Create(src.Email)))
            .ForMember(dest => dest.Password,
                opt => opt.MapFrom(src => Password.FromHash(src.PasswordHash, src.PasswordSalt)))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
            .ForMember(dest => dest.PhoneNumber,
                opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.PhoneNumber)
                        ? PhoneNumber.Create(src.PhoneNumber, src.PhoneCountryCode)
                        : null))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Bio))
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => src.TwoFactorEnabled))
            .ForMember(dest => dest.TwoFactorSecret, opt => opt.MapFrom(src => src.TwoFactorSecret))
            .ForMember(dest => dest.TwoFactorBackupCodes, opt => opt.MapFrom(src => src.TwoFactorBackupCodes))
            .ForMember(dest => dest.TwoFactorSetupDate, opt => opt.MapFrom(src => src.TwoFactorSetupDate))
            .ForMember(dest => dest.PendingTwoFactorSecret, opt => opt.MapFrom(src => src.PendingTwoFactorSecret))
            .ForMember(dest => dest.PendingTwoFactorSecretExpiresAt,
                opt => opt.MapFrom(src => src.PendingTwoFactorSecretExpiresAt))
            .ForMember(dest => dest.PasswordResetToken, opt => opt.MapFrom(src => src.PasswordResetToken))
            .ForMember(dest => dest.PasswordResetTokenExpiresAt,
                opt => opt.MapFrom(src => src.PasswordResetTokenExpiresAt))
            .ForMember(dest => dest.PasswordChangedAt, opt => opt.MapFrom(src => src.PasswordChangedAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => src.IsEmailVerified))
            .ForMember(dest => dest.IsPhoneVerified, opt => opt.MapFrom(src => src.IsPhoneVerified))
            .ForMember(dest => dest.EmailVerifiedAt, opt => opt.MapFrom(src => src.EmailVerifiedAt))
            .ForMember(dest => dest.PhoneVerifiedAt, opt => opt.MapFrom(src => src.PhoneVerifiedAt))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt))
            .ForMember(dest => dest.LastLoginIp, opt => opt.MapFrom(src => src.LastLoginIp))
            .ForMember(dest => dest.FailedLoginAttempts, opt => opt.MapFrom(src => src.FailedLoginAttempts))
            .ForMember(dest => dest.LockoutEnd, opt => opt.MapFrom(src => src.LockoutEnd))
            .ForMember(dest => dest.PreferredLanguage, opt => opt.MapFrom(src => src.PreferredLanguage))
            .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone))
            .ForMember(dest => dest.ReceiveNotifications, opt => opt.MapFrom(src => src.ReceiveNotifications))
            .ForMember(dest => dest.ReceiveMarketingEmails, opt => opt.MapFrom(src => src.ReceiveMarketingEmails))
            .ForMember(dest => dest.UserDevices, opt => opt.MapFrom(src => src.UserDevices))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        // Authorization mappings
        CreateMap<Permission, PermissionEf>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Resource, opt => opt.MapFrom(src => src.Resource.Value))
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action.Value))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<PermissionEf, Permission>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Resource, opt => opt.MapFrom(src => ResourceType.Create(src.Resource)))
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src => ResourceAction.Create(src.Action)))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<Role, RoleEf>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code.Value))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src.Rank))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<RoleEf, Role>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => RoleCode.Create(src.Code)))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src.Rank))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<Resource, ResourceEf>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.Value))
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => src.NodeId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<ResourceEf, Resource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ResourceType.Create(src.Type)))
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => src.NodeId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<RolePermission, RolePermissionEf>()
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
            .ForMember(dest => dest.PermissionId, opt => opt.MapFrom(src => src.PermissionId))
            .ForMember(dest => dest.GrantedById, opt => opt.MapFrom(src => src.GrantedById));

        CreateMap<RolePermissionEf, RolePermission>()
            .ConstructUsing(src => RolePermission.Create(src.RoleId, src.PermissionId, src.GrantedById));

        CreateMap<UserNodeRole, UserNodeRoleEf>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => src.NodeId))
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
            .ForMember(dest => dest.StartAt, opt => opt.MapFrom(src => src.StartAt))
            .ForMember(dest => dest.EndAt, opt => opt.MapFrom(src => src.EndAt))
            .ForMember(dest => dest.AssignedById, opt => opt.MapFrom(src => src.AssignedById));

        CreateMap<UserNodeRoleEf, UserNodeRole>()
            .ConstructUsing(src => UserNodeRole.Create(src.UserId, src.NodeId, src.RoleId,
                src.StartAt, src.EndAt, src.AssignedById));

        // Organizations mappings
        CreateMap<OrgNode, OrgNodeEf>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<OrgNodeEf, OrgNode>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (OrgNodeType)src.Type))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<CompanyDetails, CompanyDetailsEf>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => src.NodeId))
            .ForMember(dest => dest.TaxCode, opt => opt.MapFrom(src => src.TaxCode != null ? src.TaxCode.Value : null))
            .ForMember(dest => dest.Domain, opt => opt.MapFrom(src => src.Domain != null ? src.Domain.Value : null))
            .ForMember(dest => dest.Address,
                opt => opt.MapFrom(src => src.Address != null ? src.Address.ToString() : null))
            .ForMember(dest => dest.EstablishedOn, opt => opt.MapFrom(src => src.EstablishedOn))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<CompanyDetailsEf, CompanyDetails>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => src.NodeId))
            .ForMember(dest => dest.TaxCode,
                opt => opt.MapFrom(src => src.TaxCode != null ? TaxCode.Create(src.TaxCode) : null))
            .ForMember(dest => dest.Domain,
                opt => opt.MapFrom(src => src.Domain != null ? DomainName.Create(src.Domain) : null))
            .ForMember(dest => dest.Address,
                opt => opt.MapFrom(src => (Address?)null)) // TODO: Implement proper address parsing
            .ForMember(dest => dest.EstablishedOn, opt => opt.MapFrom(src => src.EstablishedOn))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<DepartmentDetails, DepartmentDetailsEf>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => src.NodeId))
            .ForMember(dest => dest.CostCenter,
                opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.Value : null))
            .ForMember(dest => dest.Headcount, opt => opt.MapFrom(src => src.Headcount))
            .ForMember(dest => dest.BudgetYear, opt => opt.MapFrom(src => src.BudgetYear))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<DepartmentDetailsEf, DepartmentDetails>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => src.NodeId))
            .ForMember(dest => dest.CostCenter,
                opt => opt.MapFrom(src => src.CostCenter != null ? CostCenter.Create(src.CostCenter) : null))
            .ForMember(dest => dest.Headcount, opt => opt.MapFrom(src => src.Headcount))
            .ForMember(dest => dest.BudgetYear, opt => opt.MapFrom(src => src.BudgetYear))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<UserDevice, UserDeviceEf>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.DeviceId))
            .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.DeviceName))
            .ForMember(dest => dest.DeviceType, opt => opt.MapFrom(src => src.DeviceType))
            .ForMember(dest => dest.UserAgent, opt => opt.MapFrom(src => src.UserAgent))
            .ForMember(dest => dest.IpAddress,
                opt => opt.MapFrom(src => src.IpAddress != null ? src.IpAddress.Value : null))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.Browser, opt => opt.MapFrom(src => src.Browser))
            .ForMember(dest => dest.OperatingSystem, opt => opt.MapFrom(src => src.OperatingSystem))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt))
            .ForMember(dest => dest.LastActivityAt, opt => opt.MapFrom(src => src.LastActivityAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsTrusted, opt => opt.MapFrom(src => src.IsTrusted))
            .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.RefreshToken))
            .ForMember(dest => dest.RefreshTokenExpiresAt, opt => opt.MapFrom(src => src.RefreshTokenExpiresAt))
            .ForMember(dest => dest.ActiveAccessTokenJti, opt => opt.MapFrom(src => src.ActiveAccessTokenJti))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<UserDeviceEf, UserDevice>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.DeviceId))
            .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.DeviceName))
            .ForMember(dest => dest.DeviceType, opt => opt.MapFrom(src => src.DeviceType))
            .ForMember(dest => dest.UserAgent, opt => opt.MapFrom(src => src.UserAgent))
            .ForMember(dest => dest.IpAddress,
                opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.IpAddress) ? IPAddress.Create(src.IpAddress) : null))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.Browser, opt => opt.MapFrom(src => src.Browser))
            .ForMember(dest => dest.OperatingSystem, opt => opt.MapFrom(src => src.OperatingSystem))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt))
            .ForMember(dest => dest.LastActivityAt, opt => opt.MapFrom(src => src.LastActivityAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsTrusted, opt => opt.MapFrom(src => src.IsTrusted))
            .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.RefreshToken))
            .ForMember(dest => dest.RefreshTokenExpiresAt, opt => opt.MapFrom(src => src.RefreshTokenExpiresAt))
            .ForMember(dest => dest.ActiveAccessTokenJti, opt => opt.MapFrom(src => src.ActiveAccessTokenJti))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        // SigningKey mappings
        CreateMap<SigningKey, SigningKeyEf>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.KeyId, opt => opt.MapFrom(src => src.KeyId))
            .ForMember(dest => dest.PublicKey, opt => opt.MapFrom(src => src.PublicKey))
            .ForMember(dest => dest.PrivateKey, opt => opt.MapFrom(src => src.PrivateKey))
            .ForMember(dest => dest.Algorithm, opt => opt.MapFrom(src => src.Algorithm))
            .ForMember(dest => dest.KeySize, opt => opt.MapFrom(src => src.KeySize))
            .ForMember(dest => dest.Use, opt => opt.MapFrom(src => src.Use))
            .ForMember(dest => dest.KeyType, opt => opt.MapFrom(src => src.KeyType))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsRevoked, opt => opt.MapFrom(src => src.IsRevoked))
            .ForMember(dest => dest.RevokedAt, opt => opt.MapFrom(src => src.RevokedAt))
            .ForMember(dest => dest.RevocationReason, opt => opt.MapFrom(src => src.RevocationReason))
            .ForMember(dest => dest.ExpiresAt, opt => opt.MapFrom(src => src.ExpiresAt))
            .ForMember(dest => dest.LastUsedAt, opt => opt.MapFrom(src => src.LastUsedAt))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<SigningKeyEf, SigningKey>()
            .ConstructUsing(src => SigningKey.Create(
                src.KeyId,
                src.PublicKey,
                src.PrivateKey,
                src.Algorithm,
                src.KeySize,
                src.ExpiresAt,
                src.Description))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsRevoked, opt => opt.MapFrom(src => src.IsRevoked))
            .ForMember(dest => dest.RevokedAt, opt => opt.MapFrom(src => src.RevokedAt))
            .ForMember(dest => dest.RevocationReason, opt => opt.MapFrom(src => src.RevocationReason))
            .ForMember(dest => dest.LastUsedAt, opt => opt.MapFrom(src => src.LastUsedAt))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        // MenuItem mappings
        CreateMap<MenuItem, MenuItemEf>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.Icon))
            .ForMember(dest => dest.Route, opt => opt.MapFrom(src => src.Route))
            .ForMember(dest => dest.ExternalUrl, opt => opt.MapFrom(src => src.ExternalUrl))
            .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Level))
            .ForMember(dest => dest.IsVisible, opt => opt.MapFrom(src => src.IsVisible))
            .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled))
            .ForMember(dest => dest.RequiredPermission, opt => opt.MapFrom(src => src.RequiredPermission))
            .ForMember(dest => dest.RequiredRoles, opt => opt.MapFrom(src => src.RequiredRoles))
            .ForMember(dest => dest.OpenInNewTab, opt => opt.MapFrom(src => src.OpenInNewTab))
            .ForMember(dest => dest.CssClass, opt => opt.MapFrom(src => src.CssClass))
            .ForMember(dest => dest.Badge, opt => opt.MapFrom(src => src.Badge))
            .ForMember(dest => dest.BadgeVariant, opt => opt.MapFrom(src => src.BadgeVariant))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata))
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<MenuItemEf, MenuItem>()
            .ConstructUsing(src => MenuItem.Create(
                src.Code,
                src.Name,
                src.Description,
                src.Icon,
                src.Route,
                src.ParentId,
                src.SortOrder,
                src.RequiredPermission,
                src.RequiredRoles))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ExternalUrl, opt => opt.Ignore())
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Level))
            .ForMember(dest => dest.IsVisible, opt => opt.MapFrom(src => src.IsVisible))
            .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled))
            .ForMember(dest => dest.OpenInNewTab, opt => opt.MapFrom(src => src.OpenInNewTab))
            .ForMember(dest => dest.CssClass, opt => opt.MapFrom(src => src.CssClass))
            .ForMember(dest => dest.Badge, opt => opt.MapFrom(src => src.Badge))
            .ForMember(dest => dest.BadgeVariant, opt => opt.MapFrom(src => src.BadgeVariant))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata))
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        // SystemSetting mappings
        CreateMap<SystemSetting, SystemSettingEf>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
            .ForMember(dest => dest.DefaultValue, opt => opt.MapFrom(src => src.DefaultValue))
            .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => (int)src.DataType))
            .ForMember(dest => dest.Group, opt => opt.MapFrom(src => src.Group))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.IsVisible, opt => opt.MapFrom(src => src.IsVisible))
            .ForMember(dest => dest.IsEditable, opt => opt.MapFrom(src => src.IsEditable))
            .ForMember(dest => dest.IsSensitive, opt => opt.MapFrom(src => src.IsSensitive))
            .ForMember(dest => dest.IsRequired, opt => opt.MapFrom(src => src.IsRequired))
            .ForMember(dest => dest.ValidationRules, opt => opt.MapFrom(src => src.ValidationRules))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata))
            .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.LastModifiedBy))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<SystemSettingEf, SystemSetting>()
            .ConstructUsing(src => SystemSetting.Create(
                src.Key,
                src.DisplayName,
                src.Value,
                src.DefaultValue,
                (SettingDataType)src.DataType,
                src.Group,
                src.Description,
                src.SortOrder,
                src.IsRequired,
                src.IsSensitive))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.IsVisible, opt => opt.MapFrom(src => src.IsVisible))
            .ForMember(dest => dest.IsEditable, opt => opt.MapFrom(src => src.IsEditable))
            .ForMember(dest => dest.ValidationRules, opt => opt.MapFrom(src => src.ValidationRules))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata))
            .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.LastModifiedBy))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        // EmailTemplate mappings
        CreateMap<EmailTemplate, EmailTemplateEf>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Subject))
            .ForMember(dest => dest.HtmlBody, opt => opt.MapFrom(src => src.HtmlBody))
            .ForMember(dest => dest.PlainTextBody, opt => opt.MapFrom(src => src.PlainTextBody))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.AvailablePlaceholders, opt => opt.MapFrom(src => src.AvailablePlaceholders))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsSystem, opt => opt.MapFrom(src => src.IsSystem))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => (int)src.Category))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<EmailTemplateEf, EmailTemplate>()
            .ConstructUsing(src => EmailTemplate.Create(
                src.Code,
                src.Name,
                src.Subject,
                src.HtmlBody,
                (EmailTemplateCategory)src.Category,
                src.Description,
                src.PlainTextBody,
                src.AvailablePlaceholders,
                src.IsSystem))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));
    }
}
