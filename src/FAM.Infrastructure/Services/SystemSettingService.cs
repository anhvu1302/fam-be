using FAM.Application.Settings.DTOs;
using FAM.Application.Settings.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Entities;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Services;

/// <summary>
/// Implementation of system setting service
/// </summary>
public class SystemSettingService : ISystemSettingService
{
    private readonly ISystemSettingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SystemSettingService> _logger;

    public SystemSettingService(
        ISystemSettingRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<SystemSettingService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<SystemSettingResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetAllSettingsAsync(cancellationToken);
        return settings.Select(s => SystemSettingResponse.FromDomain(s));
    }

    public async Task<IEnumerable<PublicSettingResponse>> GetAllPublicAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetVisibleSettingsAsync(cancellationToken);
        return settings.Select(PublicSettingResponse.FromDomain);
    }

    public async Task<IEnumerable<SettingsGroupResponse>> GetGroupedAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetAllSettingsAsync(cancellationToken);
        return settings
            .GroupBy(s => s.Group)
            .OrderBy(g => g.Key)
            .Select(g => new SettingsGroupResponse(
                g.Key,
                g.Select(s => SystemSettingResponse.FromDomain(s)).ToList()));
    }

    public async Task<IEnumerable<PublicSettingsGroupResponse>> GetPublicGroupedAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetVisibleSettingsAsync(cancellationToken);
        return settings
            .GroupBy(s => s.Group)
            .OrderBy(g => g.Key)
            .Select(g => new PublicSettingsGroupResponse(
                g.Key,
                g.Select(PublicSettingResponse.FromDomain).ToList()));
    }

    public async Task<SystemSettingResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var setting = await _repository.GetByIdAsync(id, cancellationToken);
        return setting != null ? SystemSettingResponse.FromDomain(setting) : null;
    }

    public async Task<SystemSettingResponse?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await _repository.GetByKeyAsync(key, cancellationToken);
        return setting != null ? SystemSettingResponse.FromDomain(setting) : null;
    }

    public async Task<IEnumerable<SystemSettingResponse>> GetByGroupAsync(string group,
        CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetByGroupAsync(group, cancellationToken);
        return settings.Select(s => SystemSettingResponse.FromDomain(s));
    }

    public async Task<IEnumerable<string>> GetGroupsAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetGroupsAsync(cancellationToken);
    }

    public async Task<SystemSettingResponse> CreateAsync(CreateSystemSettingRequest request,
        CancellationToken cancellationToken = default)
    {
        // Check if key already exists
        if (await _repository.KeyExistsAsync(request.Key, cancellationToken: cancellationToken))
            throw new DomainException(ErrorCodes.SETTING_KEY_EXISTS);

        var setting = SystemSetting.Create(
            request.Key,
            request.DisplayName,
            request.Value,
            request.DefaultValue,
            request.DataType,
            request.Group,
            request.Description,
            request.SortOrder,
            request.IsRequired,
            request.IsSensitive);

        if (!request.IsVisible) setting.Hide();

        if (!request.IsEditable) setting.MakeReadOnly();

        if (!string.IsNullOrEmpty(request.ValidationRules)) setting.SetValidationRules(request.ValidationRules);

        if (!string.IsNullOrEmpty(request.Options)) setting.SetOptions(request.Options);

        await _repository.AddAsync(setting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created system setting: {Key}", request.Key);

        return SystemSettingResponse.FromDomain(setting);
    }

    public async Task<SystemSettingResponse> UpdateAsync(long id, UpdateSystemSettingRequest request,
        CancellationToken cancellationToken = default)
    {
        var setting = await _repository.GetByIdAsync(id, cancellationToken);
        if (setting == null) throw new NotFoundException(ErrorCodes.SETTING_NOT_FOUND, "SystemSetting", id);

        setting.Update(
            request.DisplayName,
            request.Description,
            request.DefaultValue,
            request.SortOrder);

        if (request.Value != null) setting.SetValue(request.Value);

        if (request.IsVisible.HasValue)
        {
            if (request.IsVisible.Value) setting.Show();
            else setting.Hide();
        }

        if (request.IsEditable.HasValue)
        {
            if (request.IsEditable.Value) setting.MakeEditable();
            else setting.MakeReadOnly();
        }

        if (request.ValidationRules != null) setting.SetValidationRules(request.ValidationRules);

        if (request.Options != null) setting.SetOptions(request.Options);

        _repository.Update(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated system setting: {Id}", id);

        return SystemSettingResponse.FromDomain(setting);
    }

    public async Task<SystemSettingResponse> UpdateValueAsync(string key, UpdateSettingValueRequest request,
        long? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        var setting = await _repository.GetByKeyAsync(key, cancellationToken);
        if (setting == null) throw new NotFoundException(ErrorCodes.SETTING_NOT_FOUND, "SystemSetting", key);

        if (!setting.IsEditable) throw new DomainException(ErrorCodes.SETTING_NOT_EDITABLE);

        if (setting.IsRequired && string.IsNullOrEmpty(request.Value))
            throw new DomainException(ErrorCodes.SETTING_VALUE_REQUIRED);

        setting.SetValue(request.Value, modifiedBy);

        _repository.Update(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated system setting value: {Key}", key);

        return SystemSettingResponse.FromDomain(setting);
    }

    public async Task BulkUpdateAsync(BulkUpdateSettingsRequest request, long? modifiedBy = null,
        CancellationToken cancellationToken = default)
    {
        // Validate all settings exist and are editable
        foreach (var key in request.Settings.Keys)
        {
            var setting = await _repository.GetByKeyAsync(key, cancellationToken);
            if (setting == null) throw new NotFoundException(ErrorCodes.SETTING_NOT_FOUND, "SystemSetting", key);

            if (!setting.IsEditable)
                throw new DomainException(ErrorCodes.SETTING_NOT_EDITABLE, $"Setting '{key}' is not editable");

            if (setting.IsRequired && string.IsNullOrEmpty(request.Settings[key]))
                throw new DomainException(ErrorCodes.SETTING_VALUE_REQUIRED, $"Setting '{key}' requires a value");
        }

        await _repository.BulkUpdateAsync(request.Settings, modifiedBy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bulk updated {Count} system settings", request.Settings.Count);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var setting = await _repository.GetByIdAsync(id, cancellationToken);
        if (setting == null) throw new NotFoundException(ErrorCodes.SETTING_NOT_FOUND, "SystemSetting", id);

        _repository.Delete(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted system setting: {Id}", id);
    }

    public async Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await _repository.GetByKeyAsync(key, cancellationToken);
        return setting?.GetEffectiveValue();
    }

    public async Task<bool> GetBoolValueAsync(string key, bool defaultValue = false,
        CancellationToken cancellationToken = default)
    {
        var setting = await _repository.GetByKeyAsync(key, cancellationToken);
        return setting?.GetBoolValue(defaultValue) ?? defaultValue;
    }

    public async Task<int> GetIntValueAsync(string key, int defaultValue = 0,
        CancellationToken cancellationToken = default)
    {
        var setting = await _repository.GetByKeyAsync(key, cancellationToken);
        return setting?.GetIntValue(defaultValue) ?? defaultValue;
    }
}