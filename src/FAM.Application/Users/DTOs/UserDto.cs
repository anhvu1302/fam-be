namespace FAM.Application.DTOs.Users;

/// <summary>
/// User Data Transfer Object
/// Following REST API best practices:
/// - To-one relationships can be included via ?include parameter
/// - To-many relationships should use nested resource endpoints:
///   * GET /api/users/{id}/devices - for user's devices
///   * GET /api/users/{id}/roles - for user's node roles
/// </summary>
public class UserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // NOTE: Removed to-many navigation properties (UserDevices, UserNodeRoles)
    // Use dedicated endpoints instead:
    // - GET /api/users/{id}/devices
    // - GET /api/users/{id}/roles
    
    // If User had to-one relationships (e.g., Profile, Address), they would go here
    // Example: public UserProfileDto? Profile { get; set; }
}

/// <summary>
/// User Device DTO
/// </summary>
public class UserDeviceDto
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Location { get; set; }
    public DateTime LastLoginAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsTrusted { get; set; }
}

/// <summary>
/// User Node Role DTO
/// </summary>
public class UserNodeRoleDto
{
    public long Id { get; set; }
    public long NodeId { get; set; }
    public long RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? NodeName { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    
    // Optional nested includes
    public RoleDto? Role { get; set; }
    public OrgNodeDto? Node { get; set; }
}

/// <summary>
/// Role DTO (simplified)
/// </summary>
public class RoleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Organization Node DTO (simplified)
/// </summary>
public class OrgNodeDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
}