using FAM.Application.Querying.Extensions;
using FAM.Application.Settings;
using FAM.Domain.Authorization;
using FAM.WebApi.Contracts.Common;
using FAM.WebApi.Mappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Permission management controller
/// Manages system permissions
/// </summary>
[ApiController]
[Route("api/permissions")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PermissionsController> _logger;
    private readonly PaginationSettings _pagination;

    public PermissionsController(IMediator mediator, ILogger<PermissionsController> logger,
        PaginationSettings pagination)
    {
        _mediator = mediator;
        _logger = logger;
        _pagination = pagination;
    }

    /// <summary>
    /// Get paginated list of permissions with advanced filtering and sorting
    /// </summary>
    /// <param name="parameters">Query parameters for pagination, filtering, sorting, field selection and includes</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissions([FromQuery] PaginationQueryParameters parameters)
    {
        var query = PermissionMappers.ToQuery(parameters.ToQueryRequest());
        var result = await _mediator.Send(query);

        // Apply field selection if requested
        var fields = parameters.GetFieldsArray();
        if (fields != null && fields.Length > 0)
        {
            var selectedResult = result.SelectFieldsToResponse(fields);
            return Ok(selectedResult);
        }

        return Ok(result.ToPagedResponse());
    }

    /// <summary>
    /// Get all available permission definitions
    /// Returns predefined permissions from the system
    /// </summary>
    [HttpGet("definitions")]
    [ProducesResponseType(typeof(IReadOnlyList<object>), StatusCodes.Status200OK)]
    public ActionResult GetPermissionDefinitions()
    {
        var permissions = Permissions.All
            .Select(p => new
            {
                Resource = p.Resource,
                Action = p.Action,
                Description = p.Description,
                PermissionKey = $"{p.Resource}:{p.Action}"
            })
            .ToList();

        return Ok(permissions);
    }

    /// <summary>
    /// Get available resources
    /// </summary>
    [HttpGet("resources")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public ActionResult GetResources()
    {
        var resources = Resources.All;
        return Ok(resources);
    }

    /// <summary>
    /// Get available actions
    /// </summary>
    [HttpGet("actions")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public ActionResult GetActions()
    {
        var actions = Actions.All;
        return Ok(actions);
    }
}