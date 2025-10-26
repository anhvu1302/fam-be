using MediatR;
using Microsoft.AspNetCore.Mvc;
using FAM.Application.Users.Commands;
using FAM.Application.Users.Queries;
using FAM.Application.DTOs.Users;
using FAM.Application.Querying;
using FAM.Application.Querying.Extensions;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Users API Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get paginated list of users with advanced filtering and sorting
    /// </summary>
    /// <param name="filter">Filter expression (e.g., "isDeleted == false", "username @contains('john') and email @endswith('@example.com')")</param>
    /// <param name="sort">Sort expression (e.g., "createdAt", "-username,email" for descending username then ascending email)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="fields">Fields to return (comma-separated, e.g., "id,username,email"). If not specified, returns all fields.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PageResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PageResult<Dictionary<string, object?>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? filter = null,
        [FromQuery] string? sort = null,
        [FromQuery] int? page = 1,
        [FromQuery] int? pageSize = 10,
        [FromQuery] string? fields = null)
    {
        var query = new GetUsersQuery
        {
            Filter = filter,
            Sort = sort,
            Page = page ?? 1,
            PageSize = pageSize ?? 10,
            Fields = string.IsNullOrWhiteSpace(fields) ? null : fields.Split(',', StringSplitOptions.RemoveEmptyEntries)
        };

        var result = await _mediator.Send(query);

        // Apply field selection if requested
        if (query.Fields != null && query.Fields.Length > 0)
        {
            var selectedResult = result.SelectFields(query.Fields);
            return Ok(selectedResult);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(long id)
    {
        var query = new GetUserByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(long id, [FromBody] UpdateUserCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(long id)
    {
        var command = new DeleteUserCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}