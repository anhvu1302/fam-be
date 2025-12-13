using FAM.Application.Settings.DTOs;

namespace FAM.Application.Settings.Services;

/// <summary>
/// Service for system settings management
/// </summary>
public interface ISystemSettingService
{
    /// <summary>
    /// Get all settings (no pagination - for caching)
    /// </summary>
    Task<IEnumerable<SystemSettingResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all public settings (for FE caching, no sensitive data)
    /// </summary>
    Task<IEnumerable<PublicSettingResponse>> GetAllPublicAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get settings grouped by category
    /// </summary>
    Task<IEnumerable<SettingsGroupResponse>> GetGroupedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get public settings grouped by category
    /// </summary>
    Task<IEnumerable<PublicSettingsGroupResponse>> GetPublicGroupedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get setting by ID
    /// </summary>
    Task<SystemSettingResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get setting by key
    /// </summary>
    Task<SystemSettingResponse?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get settings by group
    /// </summary>
    Task<IEnumerable<SystemSettingResponse>> GetByGroupAsync(string group,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all groups
    /// </summary>
    Task<IEnumerable<string>> GetGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new setting
    /// </summary>
    Task<SystemSettingResponse> CreateAsync(CreateSystemSettingRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a setting
    /// </summary>
    Task<SystemSettingResponse> UpdateAsync(long id, UpdateSystemSettingRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update setting value by key
    /// </summary>
    Task<SystemSettingResponse> UpdateValueAsync(string key, UpdateSettingValueRequest request, long? modifiedBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update settings
    /// </summary>
    Task BulkUpdateAsync(BulkUpdateSettingsRequest request, long? modifiedBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a setting
    /// </summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get setting value as string
    /// </summary>
    Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get setting value as bool
    /// </summary>
    Task<bool> GetBoolValueAsync(string key, bool defaultValue = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get setting value as int
    /// </summary>
    Task<int> GetIntValueAsync(string key, int defaultValue = 0, CancellationToken cancellationToken = default);
}
