using FAM.Application.Querying;
using FAM.Application.Settings.Commands.CreateSystemSetting;
using FAM.Application.Settings.Commands.DeleteSystemSetting;
using FAM.Application.Settings.Commands.UpdateSystemSetting;
using FAM.Application.Settings.Queries.GetPublicSettings;
using FAM.Application.Settings.Queries.GetSystemSettingById;
using FAM.Application.Settings.Queries.GetSystemSettings;
using FAM.Application.Settings.Shared;
using FAM.Domain.Common.Base;
using FAM.WebApi.Contracts.Common;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Controller for System Settings management
/// </summary>
[ApiController]
[Route("api/settings")]
public class SettingsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(IMediator mediator, ILogger<SettingsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region Public Endpoints

    /// <summary>
    /// Get all public settings - returns all visible, non-sensitive settings
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<List<PublicSettingDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PublicSettingDto>>> GetPublicSettings(
        CancellationToken cancellationToken = default)
    {
        var query = new GetPublicSettingsQuery();
        List<PublicSettingDto> settings = await _mediator.Send(query, cancellationToken);
        return OkResponse(settings);
    }

    #endregion

    #region Admin CRUD Endpoints

    /// <summary>
    /// Get paginated list of settings (admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(PageResult<SystemSettingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PageResult<SystemSettingDto>>> GetSettings(
        [FromQuery] QueryRequest queryRequest,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSystemSettingsQuery(queryRequest);
        PageResult<SystemSettingDto> result = await _mediator.Send(query, cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Get a specific setting by ID
    /// </summary>
    [HttpGet("{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(SystemSettingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SystemSettingDto>> GetSettingById(long id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSystemSettingByIdQuery(id);
        SystemSettingDto? setting = await _mediator.Send(query, cancellationToken);
        if (setting == null)
            throw new NotFoundException(ErrorCodes.SETTING_NOT_FOUND, "SystemSetting", id);
        return OkResponse(setting);
    }

    /// <summary>
    /// Create a new setting
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<long>> CreateSetting(
        [FromBody] CreateSystemSettingCommand command,
        CancellationToken cancellationToken = default)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetSettingById), new { id }, id);
    }

    /// <summary>
    /// Update a setting
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSetting(
        long id,
        [FromBody] UpdateSystemSettingCommand command,
        CancellationToken cancellationToken = default)
    {
        UpdateSystemSettingCommand updateCommand = command with { Id = id };
        await _mediator.Send(updateCommand, cancellationToken);
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
        var command = new DeleteSystemSettingCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    #endregion
}
