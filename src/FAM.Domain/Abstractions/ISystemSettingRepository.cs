using FAM.Domain.Common.Entities;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Repository for SystemSetting management
/// </summary>
public interface ISystemSettingRepository : IRepository<SystemSetting>
{
    /// <summary>
    /// Get setting by key
    /// </summary>
    Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all settings (no pagination - for caching)
    /// </summary>
    Task<IReadOnlyList<SystemSetting>> GetAllSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get settings by group
    /// </summary>
    Task<IReadOnlyList<SystemSetting>> GetByGroupAsync(string group, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get visible settings (for admin UI)
    /// </summary>
    Task<IReadOnlyList<SystemSetting>> GetVisibleSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all groups
    /// </summary>
    Task<IReadOnlyList<string>> GetGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if key exists
    /// </summary>
    Task<bool> KeyExistsAsync(string key, long? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update setting value
    /// </summary>
    Task UpdateValueAsync(string key, string? value, long? modifiedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update settings
    /// </summary>
    Task BulkUpdateAsync(Dictionary<string, string?> keyValues, long? modifiedBy = null, CancellationToken cancellationToken = default);
}
