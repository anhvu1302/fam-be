using FAM.Application.Settings.DTOs;
using FAM.Application.Settings.Services;
using FAM.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Controller for System Settings management
/// </summary>
[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly ISystemSettingService _settingService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(ISystemSettingService settingService, ILogger<SettingsController> logger)
    {
        _settingService = settingService;
        _logger = logger;
    }

    #region Public Endpoints

    /// <summary>
    /// Get all public settings (for FE caching)
    /// No pagination - returns all visible, non-sensitive settings
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<PublicSettingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PublicSettingResponse>>> GetPublicSettings(
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingService.GetAllPublicAsync(cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Get all public settings grouped by category (for FE caching)
    /// </summary>
    [HttpGet("public/grouped")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<PublicSettingsGroupResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PublicSettingsGroupResponse>>> GetPublicSettingsGrouped(
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingService.GetPublicGroupedAsync(cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Get a specific public setting by key
    /// </summary>
    [HttpGet("public/{key}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicSettingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicSettingResponse>> GetPublicSettingByKey(string key,
        CancellationToken cancellationToken = default)
    {
        var value = await _settingService.GetValueAsync(key, cancellationToken);
        if (value == null)
        {
            var setting = await _settingService.GetByKeyAsync(key, cancellationToken);
            if (setting == null)
                throw new NotFoundException(ErrorCodes.SETTING_NOT_FOUND, "SystemSetting", key);
        }

        var result = await _settingService.GetByKeyAsync(key, cancellationToken);
        return Ok(new PublicSettingResponse(
            result!.Key,
            result.IsSensitive ? null : result.EffectiveValue,
            result.DataType,
            result.Group));
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Get all settings (admin only)
    /// No pagination - for admin caching
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(IEnumerable<SystemSettingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SystemSettingResponse>>> GetAllSettings(
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingService.GetAllAsync(cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Get all settings grouped by category (admin only)
    /// </summary>
    [HttpGet("grouped")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(IEnumerable<SettingsGroupResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SettingsGroupResponse>>> GetSettingsGrouped(
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingService.GetGroupedAsync(cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Get all setting groups
    /// </summary>
    [HttpGet("groups")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetGroups(CancellationToken cancellationToken = default)
    {
        var groups = await _settingService.GetGroupsAsync(cancellationToken);
        return Ok(groups);
    }

    /// <summary>
    /// Get settings by group
    /// </summary>
    [HttpGet("group/{group}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(IEnumerable<SystemSettingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SystemSettingResponse>>> GetSettingsByGroup(
        string group,
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingService.GetByGroupAsync(group, cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Get a specific setting by ID
    /// </summary>
    [HttpGet("{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(SystemSettingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SystemSettingResponse>> GetSettingById(long id,
        CancellationToken cancellationToken = default)
    {
        var setting = await _settingService.GetByIdAsync(id, cancellationToken);
        if (setting == null)
            throw new NotFoundException(ErrorCodes.SETTING_NOT_FOUND, "SystemSetting", id);
        return Ok(setting);
    }

    /// <summary>
    /// Get a specific setting by key
    /// </summary>
    [HttpGet("key/{key}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(SystemSettingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SystemSettingResponse>> GetSettingByKey(string key,
        CancellationToken cancellationToken = default)
    {
        var setting = await _settingService.GetByKeyAsync(key, cancellationToken);
        if (setting == null)
            throw new NotFoundException(ErrorCodes.SETTING_NOT_FOUND, "SystemSetting", key);
        return Ok(setting);
    }

    /// <summary>
    /// Create a new setting
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(SystemSettingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SystemSettingResponse>> CreateSetting(
        [FromBody] CreateSystemSettingRequest request,
        CancellationToken cancellationToken = default)
    {
        var setting = await _settingService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetSettingById), new { id = setting.Id }, setting);
    }

    /// <summary>
    /// Update a setting
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(SystemSettingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SystemSettingResponse>> UpdateSetting(
        long id,
        [FromBody] UpdateSystemSettingRequest request,
        CancellationToken cancellationToken = default)
    {
        var setting = await _settingService.UpdateAsync(id, request, cancellationToken);
        return Ok(setting);
    }

    /// <summary>
    /// Update setting value by key
    /// </summary>
    [HttpPatch("key/{key}/value")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(SystemSettingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SystemSettingResponse>> UpdateSettingValue(
        string key,
        [FromBody] UpdateSettingValueRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var setting = await _settingService.UpdateValueAsync(key, request, userId, cancellationToken);
        return Ok(setting);
    }

    /// <summary>
    /// Bulk update settings
    /// </summary>
    [HttpPatch("bulk")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkUpdateSettings(
        [FromBody] BulkUpdateSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        await _settingService.BulkUpdateAsync(request, userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Delete a setting
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSetting(long id, CancellationToken cancellationToken = default)
    {
        await _settingService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    #endregion

    #region Private Helpers

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    #endregion
}